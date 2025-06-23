using System.ComponentModel.DataAnnotations;

namespace backend.DTOs
{
    /// <summary>
    /// DTO used to reset a user's password using a valid reset token.
    /// </summary>
    public class ResetPasswordDto
    {
        /// <summary>
        /// Password reset token sent to the user's email.
        /// </summary>
        /// <example>eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...</example>
        [Required(ErrorMessage = "Token is required.")]
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// New password for the user's account.
        /// </summary>
        /// <example>NewP@ssw0rd!</example>
        [Required(ErrorMessage = "NewPassword is required.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "NewPassword must be between 6 and 100 characters.")]
        public string NewPassword { get; set; } = string.Empty;
    }
}