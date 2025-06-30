using backend.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace backend.Services
{
    /// <summary>
    /// Provides user‐related operations: registration, login, email confirmation,
    /// password reset and admin approval.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Registers a new user and sends a confirmation email.
        /// </summary>
        /// <param name="dto">The registration data.</param>
        /// <returns>
        /// A tuple where Success indicates whether the operation succeeded,
        /// and Result is the IActionResult to return to the client.
        /// </returns>
        Task<(bool Success, IActionResult Result)> RegisterAsync(RegisterDto dto);

        /// <summary>
        /// Authenticates a user and returns a JWT token.
        /// </summary>
        /// <param name="dto">The login credentials.</param>
        /// <returns>
        /// A tuple where Success indicates whether credentials were valid,
        /// and Result is the IActionResult (200+token or 401).
        /// </returns>
        Task<(bool Success, IActionResult Result)> LoginAsync(LoginDto dto);

        /// <summary>
        /// Confirms a user's email using a token.
        /// </summary>
        /// <param name="token">The email confirmation token.</param>
        /// <returns>
        /// A tuple where Success indicates whether confirmation succeeded,
        /// and Result is the IActionResult (200 or 404).
        /// </returns>
        Task<(bool Success, IActionResult Result)> ConfirmEmailAsync(string token);

        /// <summary>
        /// Sends a password‐reset email if the address exists.
        /// </summary>
        /// <param name="email">The user’s email address.</param>
        /// <returns>
        /// A tuple where Success is always true, and Result is a 200 OK with generic message.
        /// </returns>
        Task<(bool Success, IActionResult Result)> RequestPasswordResetAsync(string email);

        /// <summary>
        /// Resets a user’s password using a valid token.
        /// </summary>
        /// <param name="dto">Contains reset token and new password.</param>
        /// <returns>
        /// A tuple where Success indicates whether the token was valid,
        /// and Result is the IActionResult (200 or 400).
        /// </returns>
        Task<(bool Success, IActionResult Result)> ResetPasswordAsync(ResetPasswordDto dto);

        /// <summary>
        /// Approves a pending user account (Admin only).
        /// </summary>
        /// <param name="userId">The identifier of the user to approve.</param>
        /// <returns>
        /// A tuple where Success indicates whether approval succeeded,
        /// and Result is the IActionResult (200 or 404).
        /// </returns>
        Task<(bool Success, IActionResult Result)> ApproveUserAsync(int userId);
    }
}
