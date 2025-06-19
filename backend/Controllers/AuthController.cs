// Public controller dedicated to authentication and operations without prior authentication
// Exceptions: ApproveUser requires an Admin role

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

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly BiblioMateDbContext _context;
        private readonly IConfiguration _config;
        private readonly SendGridEmailService _emailService;

        public AuthController(BiblioMateDbContext context, IConfiguration config, SendGridEmailService emailService)
        {
            _context = context;
            _config = config;
            _emailService = emailService;
        }

        // POST: api/Auth/register
        /// <summary>
        /// Registers a new user and sends a confirmation email.
        /// </summary>
        /// <param name="dto">User registration data.</param>
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDTO dto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                return Conflict(new { error = "Un utilisateur avec cet email existe déjà." });

            var confirmationToken = Guid.NewGuid().ToString();

            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Address = dto.Address,
                Phone = dto.Phone,
                Role = "User",
                IsEmailConfirmed = false,
                EmailConfirmationToken = confirmationToken
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var confirmationUrl = $"http://localhost:4200/confirm-email?token={confirmationToken}";
            
            await _emailService.SendEmailAsync(user.Email, "Confirme ton inscription", 
                $"<p>Bienvenue {user.Name} !</p><p>Merci de confirmer ton email en cliquant ici : <a href='{confirmationUrl}'>Confirmer mon email</a></p>");

            return Ok("Inscription réussie. Vérifie ton email pour le lien de confirmation.");
        }

        // POST: api/Auth/login
        /// <summary>
        /// Authenticates a user and returns a JWT if credentials are valid.
        /// </summary>
        /// <param name="dto">User login credentials.</param>
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
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
        /// Confirms a user's email address using a token.
        /// </summary>
        /// <param name="token">The email confirmation token.</param>
        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string token)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.EmailConfirmationToken == token);

            if (user == null)
                return NotFound("Lien invalide ou expiré.");

            user.IsEmailConfirmed = true;
            user.EmailConfirmationToken = null;

            await _context.SaveChangesAsync();

            return Ok("✅ Ton email a bien été confirmé !");
        }

        /// <summary>
        /// Generates a JWT token for a valid user.
        /// </summary>
        /// <param name="user">The authenticated user.</param>
        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // POST: api/Auth/request-password-reset
        /// <summary>
        /// Sends a password reset email to the user if the email exists.
        /// </summary>
        /// <param name="dto">DTO containing the user's email.</param>
        [HttpPost("request-password-reset")]
        public async Task<IActionResult> RequestPasswordReset([FromBody] RequestPasswordResetDTO dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null)
                return Ok("Si cet email est enregistré, un lien de réinitialisation a été envoyé.");

            var resetToken = Guid.NewGuid().ToString();
            user.PasswordResetToken = resetToken;
            user.PasswordResetTokenExpires = DateTime.UtcNow.AddHours(1);

            await _context.SaveChangesAsync();

            var resetUrl = $"http://localhost:4200/reset-password?token={resetToken}";
            var html = $"<p>Bonjour {user.Name},</p><p>Clique ici pour réinitialiser ton mot de passe : <a href='{resetUrl}'>Réinitialiser</a></p>";

            await _emailService.SendEmailAsync(user.Email, "🔐 Réinitialisation du mot de passe", html);

            return Ok("Si cet email est enregistré, un lien de réinitialisation a été envoyé.");
        }

        // POST: api/Auth/reset-password
        /// <summary>
        /// Resets the user's password using a valid token.
        /// </summary>
        /// <param name="dto">DTO containing the reset token and new password.</param>
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDTO dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.PasswordResetToken == dto.Token &&
                u.PasswordResetTokenExpires > DateTime.UtcNow);

            if (user == null)
                return BadRequest("Token invalide ou expiré.");

            user.Password = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpires = null;

            await _context.SaveChangesAsync();

            return Ok("Mot de passe réinitialisé avec succès !");
        }

        // POST: api/Auth/approve/{id}
        /// <summary>
        /// Approves a user account. Only accessible by Admins.
        /// </summary>
        /// <param name="id">ID of the user to approve.</param>
        [Authorize(Roles = "Admin")]
        [HttpPost("approve/{id}")]
        public async Task<IActionResult> ApproveUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.IsApproved = true;
            await _context.SaveChangesAsync();

            return Ok($"L'utilisateur {user.Name} a été approuvé.");
        }
    }
}