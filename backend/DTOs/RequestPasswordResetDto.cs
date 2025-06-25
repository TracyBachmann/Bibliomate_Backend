using System.ComponentModel.DataAnnotations;

namespace backend.DTOs
{
    /// <summary>
    /// DTO used to initiate a password reset request.
    /// </summary>
    public class RequestPasswordResetDto
    {
        /// <summary>
        /// Email address of the user requesting password reset.
        /// </summary>
        /// <example>user@example.com</example>
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Email must be a valid email address.")]
        public string Email { get; set; } = string.Empty;
    }
}