using System.ComponentModel.DataAnnotations;

namespace BackendBiblioMate.DTOs
{
    /// <summary>
    /// Data Transfer Object used to resend the email confirmation message.
    /// </summary>
    public class ResendEmailConfirmationDto
    {
        /// <summary>
        /// Gets or sets the email address of the user.
        /// </summary>
        /// <example>user@example.com</example>
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Email must be a valid email address.")]
        public string Email { get; init; } = string.Empty;
    }
}
