using BackendBiblioMate.DTOs;
using BackendBiblioMate.Models.Enums;
using BackendBiblioMate.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace BackendBiblioMate.Controllers
{
    /// <summary>
    /// Provides API endpoints for user authentication and account management,
    /// including registration, login, email confirmation, password reset, and admin approval.
    /// </summary>
    [ApiController]
    [Route("api/[controller]"), Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    public class AuthsController : ControllerBase
    {
        private readonly IAuthService _authService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthsController"/> class.
        /// </summary>
        /// <param name="authService">
        /// The authentication service used for handling user auth logic.
        /// </param>
        public AuthsController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Registers a new user account.
        /// </summary>
        /// <param name="dto">
        /// The registration data including name, email, password, address, and phone.
        /// </param>
        /// <param name="cancellationToken">
        /// Token to monitor for cancellation requests.
        /// </param>
        /// <returns>
        /// <c>201 Created</c> on success,  
        /// <c>400 Bad Request</c> if validation failed.
        /// </returns>
        [AllowAnonymous]
        [HttpPost("register")]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Registers a new user account (v1)",
            Description = "Registers a new user with the provided details.",
            Tags = ["Auths"]
        )]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register(
            [FromBody] RegisterDto dto,
            CancellationToken cancellationToken)
        {
            var result = await _authService.RegisterAsync(dto, cancellationToken);
            return result.Result;
        }

        /// <summary>
        /// Authenticates a user and returns a JWT token if credentials are valid.
        /// </summary>
        /// <param name="dto">
        /// The login data including email and password.
        /// </param>
        /// <param name="cancellationToken">
        /// Token to monitor for cancellation requests.
        /// </param>
        /// <returns>
        /// <c>200 OK</c> with JWT and user info on success,  
        /// <c>401 Unauthorized</c> if credentials are invalid.
        /// </returns>
        [AllowAnonymous]
        [HttpPost("login")]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Authenticates a user and returns a JWT (v1)",
            Description = "Authenticates user credentials and returns a token.",
            Tags = ["Auths"]
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login(
            [FromBody] LoginDto dto,
            CancellationToken cancellationToken)
        {
            var result = await _authService.LoginAsync(dto, cancellationToken);
            return result.Result;
        }

        /// <summary>
        /// Confirms the user's email address using a confirmation token.
        /// </summary>
        /// <param name="token">
        /// The email confirmation token provided in the verification link.
        /// </param>
        /// <param name="cancellationToken">
        /// Token to monitor for cancellation requests.
        /// </param>
        /// <returns>
        /// <c>200 OK</c> if the email is confirmed,  
        /// <c>400 Bad Request</c> if the token is invalid or expired.
        /// </returns>
        [AllowAnonymous]
        [HttpGet("confirm-email")]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Confirms a user's email address (v1)",
            Description = "Verifies a user's email using the confirmation token.",
            Tags = ["Auths"]
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ConfirmEmail(
            [FromQuery] string token,
            CancellationToken cancellationToken)
        {
            var result = await _authService.ConfirmEmailAsync(token, cancellationToken);
            return result.Result;
        }

        /// <summary>
        /// Sends a password reset email to the specified address.
        /// </summary>
        /// <param name="dto">
        /// Contains the email address of the user requesting a password reset.
        /// </param>
        /// <param name="cancellationToken">
        /// Token to monitor for cancellation requests.
        /// </param>
        /// <returns>
        /// <c>200 OK</c> if the email was sent,  
        /// <c>404 Not Found</c> if the email is not associated with any account.
        /// </returns>
        [AllowAnonymous]
        [HttpPost("request-password-reset")]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Requests a password reset email (v1)",
            Description = "Sends a password reset link to the user's email.",
            Tags = ["Auths"]
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RequestPasswordReset(
            [FromBody] RequestPasswordResetDto dto,
            CancellationToken cancellationToken)
        {
            var result = await _authService.RequestPasswordResetAsync(dto.Email, cancellationToken);
            return result.Result;
        }

        /// <summary>
        /// Resets the user's password using a reset token and new password.
        /// </summary>
        /// <param name="dto">
        /// Contains the reset token and new password information.
        /// </param>
        /// <param name="cancellationToken">
        /// Token to monitor for cancellation requests.
        /// </param>
        /// <returns>
        /// <c>200 OK</c> on successful reset,  
        /// <c>400 Bad Request</c> if the token is invalid or expired.
        /// </returns>
        [AllowAnonymous]
        [HttpPost("reset-password")]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Resets the user's password (v1)",
            Description = "Resets a user's password using a token.",
            Tags = ["Auths"]
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResetPassword(
            [FromBody] ResetPasswordDto dto,
            CancellationToken cancellationToken)
        {
            var result = await _authService.ResetPasswordAsync(dto, cancellationToken);
            return result.Result;
        }

        /// <summary>
        /// Approves a pending user account. Only accessible to administrators.
        /// </summary>
        /// <param name="id">
        /// The identifier of the user account to approve.
        /// </param>
        /// <param name="cancellationToken">
        /// Token to monitor for cancellation requests.
        /// </param>
        /// <returns>
        /// <c>200 OK</c> if the user is approved,  
        /// <c>404 Not Found</c> if the user is not found or not pending,  
        /// <c>403 Forbidden</c> if the caller is not an administrator.
        /// </returns>
        [Authorize(Roles = UserRoles.Admin)]
        [HttpPost("approve/{id}")]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Approves a pending user account (v1)",
            Description = "Admin endpoint to approve new users.",
            Tags = ["Auths"]
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> ApproveUser(
            [FromRoute] int id,
            CancellationToken cancellationToken)
        {
            var result = await _authService.ApproveUserAsync(id, cancellationToken);
            return result.Result;
        }
    }
}