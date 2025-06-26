using backend.Data;
using backend.DTOs;
using backend.Models;
using backend.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace backend.Services
{
    /// <summary>
    /// Implements <see cref="IAuthService"/> to handle user registration,
    /// authentication, email confirmation, password reset, and approval.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly BiblioMateDbContext    _db;
        private readonly IConfiguration         _config;
        private readonly IEmailService _emailService;

        /// <summary>
        /// Creates a new instance of <see cref="AuthService"/>.
        /// </summary>
        /// <param name="db">EF Core database context.</param>
        /// <param name="config">Application configuration (JWT, Frontend URLs...).</param>
        /// <param name="emailService">Service for sending emails.</param>
        public AuthService(
            BiblioMateDbContext db,
            IConfiguration config,
            IEmailService emailService)
        {
            _db            = db;
            _config        = config;
            _emailService = emailService;
        }

        public async Task<(bool, IActionResult)> RegisterAsync(RegisterDto dto)
        {
            if (await _db.Users.AnyAsync(u => u.Email == dto.Email))
                return (false, new ConflictObjectResult(new { error = "An account with that email already exists." }));

            var token = Guid.NewGuid().ToString();
            var user = new User
            {
                Name                    = dto.Name,
                Email                   = dto.Email,
                Password                = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Address                 = dto.Address,
                Phone                   = dto.Phone,
                Role                    = UserRoles.User,
                IsEmailConfirmed        = false,
                EmailConfirmationToken  = token
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var confirmUrl = $"{_config["Frontend:BaseUrl"]}/confirm-email?token={token}";
            await _emailService.SendEmailAsync(
                user.Email,
                "Confirm your registration",
                $"<p>Welcome {user.Name}!</p><p>Please confirm your email by clicking <a href=\"{confirmUrl}\">here</a>.</p>"
            );

            return (true, new OkObjectResult("Registration successful. Check your email."));
        }

        public async Task<(bool, IActionResult)> LoginAsync(LoginDto dto)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
                return (false, new UnauthorizedObjectResult(new { error = "Invalid email or password." }));

            if (!user.IsEmailConfirmed)
                return (false, new UnauthorizedObjectResult(new { error = "Email not confirmed." }));

            if (!user.IsApproved)
                return (false, new UnauthorizedObjectResult(new { error = "Account awaiting admin approval." }));

            var jwt = GenerateJwtToken(user);
            return (true, new OkObjectResult(new { token = jwt }));
        }

        public async Task<(bool, IActionResult)> ConfirmEmailAsync(string token)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.EmailConfirmationToken == token);
            if (user == null)
                return (false, new NotFoundObjectResult("Invalid or expired confirmation link."));

            user.IsEmailConfirmed       = true;
            user.EmailConfirmationToken = null;
            await _db.SaveChangesAsync();

            return (true, new OkObjectResult("Email confirmed successfully."));
        }

        public async Task<(bool, IActionResult)> RequestPasswordResetAsync(string email)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user != null)
            {
                user.PasswordResetToken        = Guid.NewGuid().ToString();
                user.PasswordResetTokenExpires = DateTime.UtcNow.AddHours(1);
                await _db.SaveChangesAsync();

                var resetUrl = $"{_config["Frontend:BaseUrl"]}/reset-password?token={user.PasswordResetToken}";
                await _emailService.SendEmailAsync(
                    user.Email,
                    "Password Reset Request",
                    $"<p>Please reset your password by clicking <a href=\"{resetUrl}\">here</a>.</p>"
                );
            }

            return (true, new OkObjectResult("If that email is registered, you’ll receive a reset link."));
        }

        public async Task<(bool, IActionResult)> ResetPasswordAsync(ResetPasswordDto dto)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u =>
                u.PasswordResetToken == dto.Token &&
                u.PasswordResetTokenExpires > DateTime.UtcNow);

            if (user == null)
                return (false, new BadRequestObjectResult("Invalid or expired token."));

            user.Password                   = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.PasswordResetToken         = null;
            user.PasswordResetTokenExpires  = null;
            user.SecurityStamp = Guid.NewGuid().ToString();
            await _db.SaveChangesAsync();

            return (true, new OkObjectResult("Password reset successful."));
        }

        public async Task<(bool, IActionResult)> ApproveUserAsync(int userId)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user == null)
                return (false, new NotFoundResult());

            user.IsApproved = true;
            await _db.SaveChangesAsync();
            return (true, new OkObjectResult($"User '{user.Name}' approved."));
        }

        /// <summary>
        /// Generates a JWT for the given user.
        /// </summary>
        /// <param name="user">The user entity.</param>
        /// <returns>The signed JWT string.</returns>
        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email,         user.Email),
                new Claim(ClaimTypes.Role,          user.Role),
                new Claim("stamp", user.SecurityStamp)
            };

            var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer:             _config["Jwt:Issuer"],
                audience:           _config["Jwt:Audience"],
                claims:             claims,
                expires:            DateTime.UtcNow.AddHours(1),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
