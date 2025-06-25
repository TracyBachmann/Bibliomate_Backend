using System.ComponentModel.DataAnnotations;

namespace backend.DTOs
{
    /// <summary>
    /// DTO used for user authentication during login.
    /// </summary>
    public class LoginDto
    {
        /// <summary>
        /// Registered email address of the user.
        /// </summary>
        /// <example>user@example.com</example>
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Email must be a valid email address.")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Password of the user.
        /// </summary>
        /// <example>P@ssw0rd!</example>
        [Required(ErrorMessage = "Password is required.")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
        public string Password { get; set; } = string.Empty;
    }
}