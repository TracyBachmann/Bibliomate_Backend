using backend.DTOs;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.Models.Enums;

namespace backend.Controllers
{
    /// <summary>
    /// Provides API endpoints for user authentication and account management,
    /// including registration, login, email confirmation, password reset, and admin approval.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthController"/> class.
        /// </summary>
        /// <param name="authService">The authentication service used for handling user auth logic.</param>
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Registers a new user account.
        /// </summary>
        /// <param name="dto">The registration data including name, email, password, address, and phone.</param>
        /// <returns>Returns a success response if the registration was successful, or validation errors otherwise.</returns>
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            var result = await _authService.RegisterAsync(dto);
            return result.Result;
        }

        /// <summary>
        /// Authenticates a user and returns a JWT token if credentials are valid.
        /// </summary>
        /// <param name="dto">The login data including email and password.</param>
        /// <returns>Returns a JWT token and user info on success, or an unauthorized error if credentials are invalid.</returns>
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var result = await _authService.LoginAsync(dto);
            return result.Result;
        }

        /// <summary>
        /// Confirms the user's email address using a confirmation token.
        /// </summary>
        /// <param name="token">The email confirmation token provided in the verification link.</param>
        /// <returns>Returns a success message if the email is confirmed, or an error if the token is invalid or expired.</returns>
        [AllowAnonymous]
        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string token)
        {
            var result = await _authService.ConfirmEmailAsync(token);
            return result.Result;
        }

        /// <summary>
        /// Sends a password reset email to the specified address.
        /// </summary>
        /// <param name="dto">Contains the email address of the user requesting a password reset.</param>
        /// <returns>Returns a success response if the email was sent, or an error if the email is invalid or not found.</returns>
        [AllowAnonymous]
        [HttpPost("request-password-reset")]
        public async Task<IActionResult> RequestPasswordReset([FromBody] RequestPasswordResetDto dto)
        {
            var result = await _authService.RequestPasswordResetAsync(dto.Email);
            return result.Result;
        }

        /// <summary>
        /// Resets the user's password using a reset token and new password.
        /// </summary>
        /// <param name="dto">Contains the reset token and new password information.</param>
        /// <returns>Returns a success message if the password was reset, or an error if the token is invalid.</returns>
        [AllowAnonymous]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
        {
            var result = await _authService.ResetPasswordAsync(dto);
            return result.Result;
        }

        /// <summary>
        /// Approves a pending user account. Only accessible to administrators.
        /// </summary>
        /// <param name="id">The identifier of the user account to approve.</param>
        /// <returns>Returns a success message if the user is approved, or an error if the user is not found or not pending.</returns>
        [Authorize(Roles = UserRoles.Admin)]
        [HttpPost("approve/{id}")]
        public async Task<IActionResult> ApproveUser(int id)
        {
            var result = await _authService.ApproveUserAsync(id);
            return result.Result;
        }
    }
}
