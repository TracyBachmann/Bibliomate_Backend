using System.IdentityModel.Tokens.Jwt;
using System.Net;
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
        public async Task<(bool Success, IActionResult Result)> RegisterAsync(
            RegisterDto dto,
            CancellationToken cancellationToken = default)
        {
            if (await _db.Users.AnyAsync(u => u.Email == dto.Email, cancellationToken))
            {
                return (false,
                    new ConflictObjectResult(new { error = "An account with that email already exists." }));
            }

            // --- helpers for DTO compat (old/new front) ---
            static string? GetStringProp(object obj, string prop)
                => obj.GetType().GetProperty(prop)?.GetValue(obj) as string;

            static DateTime? GetDateProp(object obj, string prop)
                => obj.GetType().GetProperty(prop)?.GetValue(obj) as DateTime?;

            static IEnumerable<int>? GetIntEnumerableProp(object obj, string prop)
                => obj.GetType().GetProperty(prop)?.GetValue(obj) as IEnumerable<int>;

            var firstName = GetStringProp(dto, "FirstName");
            var lastName  = GetStringProp(dto, "LastName");
            if (firstName is null || lastName is null)
            {
                var fullName = GetStringProp(dto, "Name") ?? string.Empty;
                SplitName(fullName, out firstName, out lastName);
            }

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

            // Preferred genres
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

            // ---- Confirmation email (FR + bouton) ----
            var baseUrl = _config["Frontend:BaseUrl"] ?? "http://localhost:4200";
            var confirmUrl = $"{baseUrl}/confirm-email?token={token}";

            var subject = "Confirmez votre inscription – BiblioMate";
            var first = WebUtility.HtmlEncode(user.FirstName ?? "");

            var html = $@"
<div style=""font-family:Segoe UI,Roboto,Arial,sans-serif;max-width:560px;margin:auto;background:#f7fbff;
             border:1px solid #e6f0f7;border-radius:12px;padding:24px"">
  <h2 style=""margin:0 0 12px;color:#04446b;font-weight:600"">Bienvenue sur BiblioMate 👋</h2>
  <p style=""margin:0 0 16px;color:#0f2a3a"">Bonjour {first},</p>
  <p style=""margin:0 0 16px;color:#0f2a3a"">
    Merci de votre inscription. Pour activer votre compte, veuillez confirmer votre adresse e-mail :
  </p>
  <p style=""text-align:center;margin:24px 0"">
    <a href=""{confirmUrl}"" style=""display:inline-block;background:#fbbc05;color:#000;text-decoration:none;
          padding:12px 18px;border-radius:20px;border:2px solid #fbbc05"">
      Confirmer mon e-mail
    </a>
  </p>
  <p style=""margin:16px 0;color:#0f2a3a;font-size:14px"">
    Après la confirmation, un·e bibliothécaire devra <b>approuver</b> votre compte avant la première connexion.
  </p>
  <hr style=""border:none;border-top:1px solid #e6f0f7;margin:20px 0""/>
  <p style=""margin:0;color:#466176;font-size:12px"">
    Si le bouton ne fonctionne pas, copiez/collez ce lien :<br/>
    <a href=""{confirmUrl}"">{confirmUrl}</a>
  </p>
</div>";

            await _emailService.SendEmailAsync(user.Email, subject, html);

            return (true, new OkObjectResult("Registration successful. Check your email."));
        }

        /// <summary>
        /// Authenticates a user and returns a JWT token if successful.
        /// </summary>
        public async Task<(bool Success, IActionResult Result)> LoginAsync(
            LoginDto dto,
            CancellationToken cancellationToken = default)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email, cancellationToken);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
            {
                return (false, new UnauthorizedObjectResult(new { error = "Invalid email or password." }));
            }

            // Keep these messages as-is (frontend matches on them)
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

                var baseUrl = _config["Frontend:BaseUrl"] ?? "http://localhost:4200";
                // align with Angular route: /reinitialiser-mot-de-passe
                var resetUrl = $"{baseUrl}/reinitialiser-mot-de-passe?token={token}";

                var subject = "Réinitialisation de votre mot de passe – BiblioMate";
                var html = $@"
<div style=""font-family:Segoe UI,Roboto,Arial,sans-serif;max-width:560px;margin:auto;background:#f7fbff;
             border:1px solid #e6f0f7;border-radius:12px;padding:24px"">
  <h2 style=""margin:0 0 12px;color:#04446b;font-weight:600"">Réinitialiser votre mot de passe</h2>
  <p style=""margin:0 0 16px;color:#0f2a3a"">
    Pour choisir un nouveau mot de passe, cliquez sur le bouton ci-dessous :
  </p>
  <p style=""text-align:center;margin:24px 0"">
    <a href=""{resetUrl}"" style=""display:inline-block;background:#fbbc05;color:#000;text-decoration:none;
          padding:12px 18px;border-radius:20px;border:2px solid #fbbc05"">
      Choisir un nouveau mot de passe
    </a>
  </p>
  <hr style=""border:none;border-top:1px solid #e6f0f7;margin:20px 0""/>
  <p style=""margin:0;color:#466176;font-size:12px"">
    Si le bouton ne fonctionne pas, copiez/collez ce lien :<br/>
    <a href=""{resetUrl}"">{resetUrl}</a>
  </p>
</div>";

                await _emailService.SendEmailAsync(email, subject, html);
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

        // ----- internals -----------------------------------------------------

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

        public async Task<(bool Success, IActionResult Result)> ResendConfirmationAsync(
    string email,
    CancellationToken cancellationToken = default)
{
    if (string.IsNullOrWhiteSpace(email))
        return (false, new OkObjectResult("If needed, a new confirmation email has been sent."));

    var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

    // On ne révèle pas si le compte n'existe pas
    if (user is null)
        return (true, new OkObjectResult("If needed, a new confirmation email has been sent."));

    // Déjà confirmé -> OK idempotent
    if (user.IsEmailConfirmed)
        return (true, new OkObjectResult("If needed, a new confirmation email has been sent."));

    // (Ré)génère un token s'il n'existe plus
    if (string.IsNullOrWhiteSpace(user.EmailConfirmationToken))
    {
        user.EmailConfirmationToken = Guid.NewGuid().ToString();
        await _db.SaveChangesAsync(cancellationToken);
    }

    var baseUrl   = _config["Frontend:BaseUrl"] ?? "http://localhost:4200";
    var confirmUrl = $"{baseUrl}/confirm-email?token={user.EmailConfirmationToken}";

    // Version FR simple (tu peux peaufiner le HTML comme tu veux)
    var subject = "BiblioMate – Confirmez votre adresse e-mail";
    var html = $@"
      <div style=""font-family:Segoe UI,Arial,sans-serif;font-size:15px;line-height:1.5;color:#0f172a"">
        <p>Bonjour {(string.IsNullOrWhiteSpace(user.FirstName) ? "!" : user.FirstName + "!")}</p>
        <p>Merci de votre inscription à <strong>BiblioMate</strong>. Pour activer votre compte, cliquez sur le bouton :</p>
        <p style=""margin:24px 0"">
          <a href=""{confirmUrl}"" 
             style=""background:#04446B;color:#fff;text-decoration:none;padding:12px 18px;border-radius:8px;display:inline-block"">
            Confirmer mon e-mail
          </a>
        </p>
        <p>Si le bouton ne fonctionne pas, copiez-collez ce lien dans votre navigateur :</p>
        <p><a href=""{confirmUrl}"">{confirmUrl}</a></p>
        <hr style=""border:none;border-top:1px solid #e5e7eb;margin:24px 0""/>
        <p style=""color:#64748b"">Après confirmation, votre compte devra être approuvé par un·e bibliothécaire.</p>
      </div>";

    await _emailService.SendEmailAsync(user.Email, subject, html);

    return (true, new OkObjectResult("If needed, a new confirmation email has been sent."));

}

    }
}
