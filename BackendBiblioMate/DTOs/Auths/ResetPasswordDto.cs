using System.ComponentModel.DataAnnotations;

namespace BackendBiblioMate.DTOs
{
    /// <summary>
    /// Data Transfer Object used to reset a user's password using a valid reset token.
    /// </summary>
    public class ResetPasswordDto
    {
        /// <summary>
        /// Gets or sets the password reset token sent to the user's email.
        /// </summary>
        /// <example>eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...</example>
        [Required(ErrorMessage = "Token is required.")]
        public string Token { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the new password for the user's account.
        /// </summary>
        /// <remarks>
        /// Must be between 6 and 100 characters.
        /// </remarks>
        /// <example>NewP@ssw0rd!</example>
        [Required(ErrorMessage = "NewPassword is required.")]
        [MinLength(6, ErrorMessage = "NewPassword must be at least 6 characters long.")]
        [MaxLength(100, ErrorMessage = "NewPassword cannot exceed 100 characters.")]
        public string NewPassword { get; init; } = string.Empty;
    }
}