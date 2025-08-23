using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BackendBiblioMate.Data;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Interfaces;
using BackendBiblioMate.Models;
using BackendBiblioMate.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace BackendBiblioMate.Services.Users
{
    /// <summary>
    /// Implements <see cref="IAuthService"/> to handle user registration,
    /// authentication, email confirmation, password reset, and approval.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly BiblioMateDbContext _db;
        private readonly IConfiguration _config;
        private readonly IEmailService _emailService;

        /// <summary>
        /// Initializes a new instance of <see cref="AuthService"/>.
        /// </summary>
        /// <param name="db">Database context for user data.</param>
        /// <param name="config">Application configuration for URLs and JWT settings.</param>
        /// <param name="emailService">Service for sending email messages.</param>
        /// <exception cref="ArgumentNullException">Thrown if any dependency is null.</exception>
        public AuthService(
            BiblioMateDbContext db,
            IConfiguration config,
            IEmailService emailService)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        }

        /// <summary>
        /// Registers a new user, sends an email confirmation link.
        /// </summary>
        /// <param name="dto">Registration data.</param>
        /// <param name="cancellationToken">Token to monitor cancellation.</param>
        /// <returns>Tuple of (success flag, <see cref="IActionResult"/>).</returns>
        public async Task<(bool Success, IActionResult Result)> RegisterAsync(
            RegisterDto dto,
            CancellationToken cancellationToken = default)
        {
            if (await _db.Users.AnyAsync(u => u.Email == dto.Email, cancellationToken))
            {
                return (false,
                    new ConflictObjectResult(new { error = "An account with that email already exists." }));
            }

            // --- Helpers to read new/old properties safely ---
            static string? GetStringProp(object obj, string prop)
                => obj.GetType().GetProperty(prop)?.GetValue(obj) as string;

            static DateTime? GetDateProp(object obj, string prop)
                => obj.GetType().GetProperty(prop)?.GetValue(obj) as DateTime?;

            static IEnumerable<int>? GetIntEnumerableProp(object obj, string prop)
                => obj.GetType().GetProperty(prop)?.GetValue(obj) as IEnumerable<int>;

            // Name handling (new: FirstName/LastName, old: Name)
            var firstName = GetStringProp(dto, "FirstName");
            var lastName  = GetStringProp(dto, "LastName");
            if (firstName is null || lastName is null)
            {
                var fullName = GetStringProp(dto, "Name") ?? string.Empty;
                SplitName(fullName, out firstName, out lastName);
            }

            // Address handling (new: Address1/Address2, old: Address)
            var address1 = GetStringProp(dto, "Address1") ?? GetStringProp(dto, "Address") ?? string.Empty;
            var address2 = GetStringProp(dto, "Address2");

            var dateOfBirth      = GetDateProp(dto, "DateOfBirth");
            var profileImagePath = GetStringProp(dto, "ProfileImagePath");
            var favoriteGenreIds = GetIntEnumerableProp(dto, "FavoriteGenreIds");

            var token = Guid.NewGuid().ToString();
            var user = new User
            {
                FirstName              = firstName ?? string.Empty,
                LastName               = lastName ?? string.Empty,
                Email                  = dto.Email,
                Password               = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Address1               = address1,
                Address2               = address2,
                Phone                  = dto.Phone,
                DateOfBirth            = dateOfBirth,
                ProfileImagePath       = profileImagePath,
                Role                   = UserRoles.User,
                IsEmailConfirmed       = false,
                EmailConfirmationToken = token
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync(cancellationToken);

            // Initialize preferred genres if provided
            if (favoriteGenreIds is not null && favoriteGenreIds.Any())
            {
                foreach (var genreId in favoriteGenreIds.Distinct())
                {
                    _db.UserGenres.Add(new UserGenre
                    {
                        UserId = user.UserId,
                        GenreId = genreId
                    });
                }
                await _db.SaveChangesAsync(cancellationToken);
            }

            var confirmUrl = $"{_config["Frontend:BaseUrl"]}/confirm-email?token={token}";
            await _emailService.SendEmailAsync(
                user.Email,
                "Confirm your registration",
                $"<p>Welcome {user.FirstName}!</p><p>Please confirm your email by clicking <a href=\"{confirmUrl}\">here</a>.</p>"
            );

            return (true, new OkObjectResult("Registration successful. Check your email."));
        }

        /// <summary>
        /// Authenticates a user and returns a JWT token if successful.
        /// </summary>
        /// <param name="dto">Login credentials.</param>
        /// <param name="cancellationToken">Token to monitor cancellation.</param>
        /// <returns>Tuple of (success flag, <see cref="IActionResult"/> containing token or error).</returns>
        public async Task<(bool Success, IActionResult Result)> LoginAsync(
            LoginDto dto,
            CancellationToken cancellationToken = default)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email, cancellationToken);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
            {
                return (false, new UnauthorizedObjectResult(new { error = "Invalid email or password." }));
            }

            if (!user.IsEmailConfirmed)
            {
                return (false, new UnauthorizedObjectResult(new { error = "Email not confirmed." }));
            }

            if (!user.IsApproved)
            {
                return (false, new UnauthorizedObjectResult(new { error = "Account awaiting admin approval." }));
            }

            var jwt = GenerateJwtToken(user);
            return (true, new OkObjectResult(new { token = jwt }));
        }

        /// <summary>Confirms a user's email using the provided token.</summary>
        public async Task<(bool Success, IActionResult Result)> ConfirmEmailAsync(
            string token,
            CancellationToken cancellationToken = default)
        {
            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.EmailConfirmationToken == token, cancellationToken);

            if (user == null)
                return (false, new NotFoundObjectResult("Invalid or expired confirmation link."));

            user.IsEmailConfirmed = true;
            user.EmailConfirmationToken = null;
            await _db.SaveChangesAsync(cancellationToken);

            return (true, new OkObjectResult("Email confirmed successfully."));
        }

        /// <summary>Generates and emails a password reset token if the email exists.</summary>
        public async Task<(bool Success, IActionResult Result)> RequestPasswordResetAsync(
            string email,
            CancellationToken cancellationToken = default)
        {
            var token = Guid.NewGuid().ToString();
            var expires = DateTime.UtcNow.AddHours(1);

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
            if (user != null)
            {
                user.PasswordResetToken = token;
                user.PasswordResetTokenExpires = expires;
                await _db.SaveChangesAsync(cancellationToken);

                var resetUrl = $"{_config["Frontend:BaseUrl"]}/reset-password?token={token}";
                await _emailService.SendEmailAsync(
                    email,
                    "Password Reset Request",
                    $"<p>Please reset your password by clicking <a href=\"{resetUrl}\">here</a>.</p>"
                );
            }

            // Always return OK to avoid revealing whether email exists
            return (true, new OkObjectResult("If that email is registered, you’ll receive a reset link."));
        }

        /// <summary>Resets a user's password using a valid reset token.</summary>
        public async Task<(bool Success, IActionResult Result)> ResetPasswordAsync(
            ResetPasswordDto dto,
            CancellationToken cancellationToken = default)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u =>
                u.PasswordResetToken == dto.Token &&
                u.PasswordResetTokenExpires > DateTime.UtcNow,
                cancellationToken);

            if (user == null)
            {
                return (false, new BadRequestObjectResult("Invalid or expired token."));
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpires = null;
            user.SecurityStamp = Guid.NewGuid().ToString();
            await _db.SaveChangesAsync(cancellationToken);

            return (true, new OkObjectResult("Password reset successful."));
        }

        /// <summary>Approves a pending user account.</summary>
        public async Task<(bool Success, IActionResult Result)> ApproveUserAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);
            if (user == null)
                return (false, new NotFoundResult());

            user.IsApproved = true;
            await _db.SaveChangesAsync(cancellationToken);

            return (true, new OkObjectResult("User approved successfully."));
        }

        /// <summary>
        /// Generates a JWT token for the authenticated user.
        /// </summary>
        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("stamp", user.SecurityStamp)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // --- name splitter (fallback when only "Name" is present) ---
        private static void SplitName(string fullName, out string first, out string last)
        {
            fullName = (fullName ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(fullName))
            {
                first = string.Empty; last = string.Empty; return;
            }
            var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            first = parts.Length > 0 ? parts[0] : string.Empty;
            last  = parts.Length > 1 ? string.Join(' ', parts.Skip(1)) : string.Empty;
        }
    }
}
