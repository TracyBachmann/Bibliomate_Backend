using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Security.Claims;
using backend.Models;
using backend.Data;
using backend.DTOs;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using backend.Models.Enums;  

namespace backend.Controllers
{
    /// <summary>
    /// Handles registration, login, email confirmation, password reset
    /// and (admin-only) user approval.  
    /// Most actions are publicly available; only
    /// <see cref="ApproveUser(int)"/> requires the <c>Admin</c> role.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly BiblioMateDbContext _context;
        private readonly IConfiguration _config;
        private readonly SendGridEmailService _emailService;

        public AuthController(BiblioMateDbContext context,
                              IConfiguration        config,
                              SendGridEmailService  emailService)
        {
            _context     = context;
            _config      = config;
            _emailService = emailService;
        }

        // POST: api/Auth/register
        /// <summary>
        /// Registers a new user and sends a confirmation e-mail.
        /// </summary>
        /// <param name="dto">User registration data.</param>
        /// <returns>
        /// <c>200 OK</c> on success;  
        /// <c>409 Conflict</c> if the e-mail already exists.
        /// </returns>
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                return Conflict(new { error = "Un utilisateur avec cet email existe déjà." });

            var confirmationToken = Guid.NewGuid().ToString();

            var user = new User
            {
                Name                  = dto.Name,
                Email                 = dto.Email,
                Password              = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Address               = dto.Address,
                Phone                 = dto.Phone,
                Role                  = "User",
                IsEmailConfirmed      = false,
                EmailConfirmationToken = confirmationToken
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var confirmationUrl =
                $"http://localhost:4200/confirm-email?token={confirmationToken}";

            await _emailService.SendEmailAsync(
                user.Email,
                "Confirme ton inscription",
                $"<p>Bienvenue {user.Name} !</p>" +
                $"<p>Merci de confirmer ton email en cliquant ici : " +
                $"<a href='{confirmationUrl}'>Confirmer mon email</a></p>");

            return Ok("Inscription réussie. Vérifie ton email pour le lien de confirmation.");
        }

        // POST: api/Auth/login
        /// <summary>
        /// Authenticates a user and returns a JWT.
        /// </summary>
        /// <param name="dto">User login credentials.</param>
        /// <returns>
        /// <c>200 OK</c> with a JWT on success;  
        /// <c>401 Unauthorized</c> if credentials are invalid, e-mail unconfirmed,
        /// or the account is not yet approved.
        /// </returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _context.Users
                                     .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
                return Unauthorized(new { error = "Email ou mot de passe incorrect." });

            if (!user.IsEmailConfirmed)
                return Unauthorized(new { error = "Veuillez confirmer votre adresse email avant de vous connecter." });

            if (!user.IsApproved)
                return Unauthorized(new { error = "Votre compte est en attente de validation par un administrateur." });

            var token = GenerateJwtToken(user);
            return Ok(new { token });
        }

        // GET: api/Auth/confirm-email?token=...
        /// <summary>
        /// Confirms a user's e-mail address using a token.
        /// </summary>
        /// <param name="token">The e-mail confirmation token.</param>
        /// <returns>
        /// <c>200 OK</c> when confirmation succeeds;  
        /// <c>404 NotFound</c> if the token is invalid or expired.
        /// </returns>
        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string token)
        {
            var user = await _context.Users
                                     .FirstOrDefaultAsync(u => u.EmailConfirmationToken == token);

            if (user == null)
                return NotFound("Lien invalide ou expiré.");

            user.IsEmailConfirmed   = true;
            user.EmailConfirmationToken = null;

            await _context.SaveChangesAsync();

            return Ok("Ton email a bien été confirmé !");
        }

        // POST: api/Auth/request-password-reset
        /// <summary>
        /// Sends a password-reset e-mail if the address exists.
        /// </summary>
        /// <param name="dto">DTO containing the user’s e-mail.</param>
        /// <returns><c>200 OK</c> (always) with a generic message.</returns>
        [HttpPost("request-password-reset")]
        public async Task<IActionResult> RequestPasswordReset([FromBody] RequestPasswordResetDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null)
                return Ok("Si cet email est enregistré, un lien de réinitialisation a été envoyé.");

            var resetToken = Guid.NewGuid().ToString();
            user.PasswordResetToken        = resetToken;
            user.PasswordResetTokenExpires = DateTime.UtcNow.AddHours(1);

            await _context.SaveChangesAsync();

            var resetUrl =
                $"http://localhost:4200/reset-password?token={resetToken}";
            var html =
                $"<p>Bonjour {user.Name},</p>" +
                $"<p>Clique ici pour réinitialiser ton mot de passe : " +
                $"<a href='{resetUrl}'>Réinitialiser</a></p>";

            await _emailService.SendEmailAsync(
                user.Email,
                "Réinitialisation du mot de passe",
                html);

            return Ok("Si cet email est enregistré, un lien de réinitialisation a été envoyé.");
        }

        // POST: api/Auth/reset-password
        /// <summary>
        /// Resets a user’s password using a valid token.
        /// </summary>
        /// <param name="dto">DTO containing the reset token and new password.</param>
        /// <returns>
        /// <c>200 OK</c> on success;  
        /// <c>400 BadRequest</c> if the token is invalid or expired.
        /// </returns>
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.PasswordResetToken == dto.Token &&
                u.PasswordResetTokenExpires > DateTime.UtcNow);

            if (user == null)
                return BadRequest("Token invalide ou expiré.");

            user.Password                = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.PasswordResetToken      = null;
            user.PasswordResetTokenExpires = null;

            await _context.SaveChangesAsync();

            return Ok("Mot de passe réinitialisé avec succès !");
        }

        // POST: api/Auth/approve/{id}
        /// <summary>
        /// Approves a user account (Admin only).
        /// </summary>
        /// <param name="id">Identifier of the user to approve.</param>
        /// <returns>
        /// <c>200 OK</c> on success;  
        /// <c>404 NotFound</c> if the user does not exist.
        /// </returns>
        [Authorize(Roles = $"{UserRoles.Admin}")]
        [HttpPost("approve/{id}")]
        public async Task<IActionResult> ApproveUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            user.IsApproved = true;
            await _context.SaveChangesAsync();

            return Ok($"L'utilisateur {user.Name} a été approuvé.");
        }

        // (private helper)
        /// <summary>
        /// Generates a signed JWT for the specified user.
        /// </summary>
        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email,         user.Email),
                new Claim(ClaimTypes.Role,          user.Role)
            };

            var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer:            _config["Jwt:Issuer"],
                audience:          _config["Jwt:Audience"],
                claims:            claims,
                expires:           DateTime.UtcNow.AddHours(1),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}