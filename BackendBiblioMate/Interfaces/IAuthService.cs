using BackendBiblioMate.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace BackendBiblioMate.Interfaces
{
    /// <summary>
    /// Provides user-related operations such as registration, authentication,
    /// email confirmation, password reset, and administrative approval.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Registers a new user.
        /// </summary>
        /// <param name="dto">Registration data transfer object containing user details.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> that, when completed, yields a tuple:
        /// <list type="bullet">
        ///   <item>
        ///     <description><c>Success</c>: <c>true</c> if registration succeeded; <c>false</c> otherwise.</description>
        ///   </item>
        ///   <item>
        ///     <description><see cref="IActionResult"/>: The HTTP response to return (e.g. <c>BadRequest</c>, <c>Created</c>).</description>
        ///   </item>
        /// </list>
        /// </returns>
        Task<(bool Success, IActionResult Result)> RegisterAsync(
            RegisterDto dto,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Authenticates a user and issues a token.
        /// </summary>
        /// <param name="dto">Login data transfer object with credentials.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> yielding:
        /// <list type="bullet">
        ///   <item>
        ///     <description><c>Success</c>: <c>true</c> if credentials are valid; <c>false</c> otherwise.</description>
        ///   </item>
        ///   <item>
        ///     <description><see cref="IActionResult"/>: The HTTP response (e.g. <c>Unauthorized</c>, <c>Ok</c> with token).</description>
        ///   </item>
        /// </list>
        /// </returns>
        Task<(bool Success, IActionResult Result)> LoginAsync(
            LoginDto dto,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Confirms a user’s email address using a token.
        /// </summary>
        /// <param name="token">The email confirmation token.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> yielding:
        /// <list type="bullet">
        ///   <item>
        ///     <description><c>Success</c>: <c>true</c> if email was confirmed; <c>false</c> otherwise.</description>
        ///   </item>
        ///   <item>
        ///     <description><see cref="IActionResult"/>: The HTTP response (e.g. <c>BadRequest</c>, <c>Ok</c>).</description>
        ///   </item>
        /// </list>
        /// </returns>
        Task<(bool Success, IActionResult Result)> ConfirmEmailAsync(
            string token,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Initiates a password reset by sending a reset link to the user’s email.
        /// </summary>
        /// <param name="email">The user’s email address.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> yielding:
        /// <list type="bullet">
        ///   <item>
        ///     <description><c>Success</c>: <c>true</c> if the reset email was sent; <c>false</c> otherwise.</description>
        ///   </item>
        ///   <item>
        ///     <description><see cref="IActionResult"/>: The HTTP response (e.g. <c>NotFound</c>, <c>Ok</c>).</description>
        ///   </item>
        /// </list>
        /// </returns>
        Task<(bool Success, IActionResult Result)> RequestPasswordResetAsync(
            string email,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Resets the user’s password using the provided token and new password.
        /// </summary>
        /// <param name="dto">Data transfer object containing the reset token and new password.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> yielding:
        /// <list type="bullet">
        ///   <item>
        ///     <description><c>Success</c>: <c>true</c> if the password was reset; <c>false</c> otherwise.</description>
        ///   </item>
        ///   <item>
        ///     <description><see cref="IActionResult"/>: The HTTP response (e.g. <c>BadRequest</c>, <c>Ok</c>).</description>
        ///   </item>
        /// </list>
        /// </returns>
        Task<(bool Success, IActionResult Result)> ResetPasswordAsync(
            ResetPasswordDto dto,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Approves a newly registered user, granting them access.
        /// </summary>
        /// <param name="userId">Identifier of the user to approve.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> yielding:
        /// <list type="bullet">
        ///   <item>
        ///     <description><c>Success</c>: <c>true</c> if the user was approved; <c>false</c> if not found.</description>
        ///   </item>
        ///   <item>
        ///     <description><see cref="IActionResult"/>: The HTTP response (e.g. <c>NotFound</c>, <c>NoContent</c>).</description>
        ///   </item>
        /// </list>
        /// </returns>
        Task<(bool Success, IActionResult Result)> ApproveUserAsync(
            int userId,
            CancellationToken cancellationToken = default);
        
        Task<(bool Success, IActionResult Result)> ResendConfirmationAsync(
            string email,
            CancellationToken cancellationToken = default);

    }
}