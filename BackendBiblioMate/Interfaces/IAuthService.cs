using BackendBiblioMate.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace BackendBiblioMate.Interfaces
{
    /// <summary>
    /// Defines authentication and user account management operations.
    /// Includes registration, login, email confirmation, password reset,
    /// user approval, and related workflows.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Registers a new user in the system.
        /// </summary>
        /// <param name="dto">Registration data transfer object containing user details.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A task producing a tuple:
        /// <list type="bullet">
        ///   <item><description><c>Success</c>: <c>true</c> if registration succeeded; otherwise <c>false</c>.</description></item>
        ///   <item><description><see cref="IActionResult"/>: The HTTP response (e.g. <c>BadRequest</c>, <c>Created</c>).</description></item>
        /// </list>
        /// </returns>
        Task<(bool Success, IActionResult Result)> RegisterAsync(
            RegisterDto dto,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Authenticates a user with credentials and issues a JWT or equivalent token.
        /// </summary>
        /// <param name="dto">Login data transfer object with email/username and password.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A task producing a tuple:
        /// <list type="bullet">
        ///   <item><description><c>Success</c>: <c>true</c> if credentials are valid; otherwise <c>false</c>.</description></item>
        ///   <item><description><see cref="IActionResult"/>: The HTTP response (e.g. <c>Unauthorized</c>, <c>Ok</c> with token).</description></item>
        /// </list>
        /// </returns>
        Task<(bool Success, IActionResult Result)> LoginAsync(
            LoginDto dto,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Confirms a user’s email address using a confirmation token.
        /// </summary>
        /// <param name="token">The unique email confirmation token.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A task producing a tuple:
        /// <list type="bullet">
        ///   <item><description><c>Success</c>: <c>true</c> if email confirmation succeeded; otherwise <c>false</c>.</description></item>
        ///   <item><description><see cref="IActionResult"/>: The HTTP response (e.g. <c>BadRequest</c>, <c>Ok</c>).</description></item>
        /// </list>
        /// </returns>
        Task<(bool Success, IActionResult Result)> ConfirmEmailAsync(
            string token,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Initiates a password reset process by sending a reset link to the given email.
        /// </summary>
        /// <param name="email">The user’s registered email address.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A task producing a tuple:
        /// <list type="bullet">
        ///   <item><description><c>Success</c>: <c>true</c> if the reset email was sent; otherwise <c>false</c>.</description></item>
        ///   <item><description><see cref="IActionResult"/>: The HTTP response (e.g. <c>NotFound</c>, <c>Ok</c>).</description></item>
        /// </list>
        /// </returns>
        Task<(bool Success, IActionResult Result)> RequestPasswordResetAsync(
            string email,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Resets the user’s password using a reset token and new password.
        /// </summary>
        /// <param name="dto">Data transfer object containing the token and the new password.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A task producing a tuple:
        /// <list type="bullet">
        ///   <item><description><c>Success</c>: <c>true</c> if the password reset succeeded; otherwise <c>false</c>.</description></item>
        ///   <item><description><see cref="IActionResult"/>: The HTTP response (e.g. <c>BadRequest</c>, <c>Ok</c>).</description></item>
        /// </list>
        /// </returns>
        Task<(bool Success, IActionResult Result)> ResetPasswordAsync(
            ResetPasswordDto dto,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Approves a newly registered user, enabling their account for login.
        /// </summary>
        /// <param name="userId">The identifier of the user to approve.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A task producing a tuple:
        /// <list type="bullet">
        ///   <item><description><c>Success</c>: <c>true</c> if the user was approved; otherwise <c>false</c>.</description></item>
        ///   <item><description><see cref="IActionResult"/>: The HTTP response (e.g. <c>NotFound</c>, <c>NoContent</c>).</description></item>
        /// </list>
        /// </returns>
        Task<(bool Success, IActionResult Result)> ApproveUserAsync(
            int userId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Resends an email confirmation message to the specified address.
        /// </summary>
        /// <param name="email">The user’s email address.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A task producing a tuple:
        /// <list type="bullet">
        ///   <item><description><c>Success</c>: <c>true</c> if the confirmation email was resent; otherwise <c>false</c>.</description></item>
        ///   <item><description><see cref="IActionResult"/>: The HTTP response (e.g. <c>NotFound</c>, <c>Ok</c>).</description></item>
        /// </list>
        /// </returns>
        Task<(bool Success, IActionResult Result)> ResendConfirmationAsync(
            string email,
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Rejects a pending user account.
        /// </summary>
        /// <param name="userId">The identifier of the user to reject.</param>
        /// <param name="reason">Optional reason for rejection.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A task producing a tuple:
        /// <list type="bullet">
        ///   <item><description><c>Success</c>: <c>true</c> if the user was rejected; otherwise <c>false</c>.</description></item>
        ///   <item><description><see cref="IActionResult"/>: The HTTP response.</description></item>
        /// </list>
        /// </returns>
        Task<(bool Success, IActionResult Result)> RejectUserAsync(
            int userId,
            string? reason = null,
            CancellationToken cancellationToken = default);
    }
}
