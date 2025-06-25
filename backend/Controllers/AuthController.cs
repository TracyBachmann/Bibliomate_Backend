using backend.DTOs;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.Models.Enums;

namespace backend.Controllers
{
    /// <summary>
    /// API endpoints for user authentication:
    /// register, login, confirm-email, password reset, and approval.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        /// <summary>
        /// Initializes a new <see cref="AuthController"/>.
        /// </summary>
        /// <param name="authService">The authentication service.</param>
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Registers a new user.
        /// </summary>
        [HttpPost("register")]
        public Task<IActionResult> Register(RegisterDto dto)
            => _authService.RegisterAsync(dto).ContinueWith(t => t.Result.Result);

        /// <summary>
        /// Logs in an existing user.
        /// </summary>
        [HttpPost("login")]
        public Task<IActionResult> Login(LoginDto dto)
            => _authService.LoginAsync(dto).ContinueWith(t => t.Result.Result);

        /// <summary>
        /// Confirms the user’s email address.
        /// </summary>
        [HttpGet("confirm-email")]
        public Task<IActionResult> ConfirmEmail([FromQuery] string token)
            => _authService.ConfirmEmailAsync(token).ContinueWith(t => t.Result.Result);

        /// <summary>
        /// Requests a password reset email.
        /// </summary>
        [HttpPost("request-password-reset")]
        public Task<IActionResult> RequestPasswordReset([FromBody] RequestPasswordResetDto dto)
            => _authService.RequestPasswordResetAsync(dto.Email).ContinueWith(t => t.Result.Result);

        /// <summary>
        /// Resets the user’s password.
        /// </summary>
        [HttpPost("reset-password")]
        public Task<IActionResult> ResetPassword(ResetPasswordDto dto)
            => _authService.ResetPasswordAsync(dto).ContinueWith(t => t.Result.Result);

        /// <summary>
        /// Approves a pending user account (Admin only).
        /// </summary>
        [Authorize(Roles = UserRoles.Admin)]
        [HttpPost("approve/{id}")]
        public Task<IActionResult> ApproveUser(int id)
            => _authService.ApproveUserAsync(id).ContinueWith(t => t.Result.Result);
    }
}
