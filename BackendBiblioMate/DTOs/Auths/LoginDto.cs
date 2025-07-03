using System.ComponentModel.DataAnnotations;

namespace BackendBiblioMate.DTOs
{
    /// <summary>
    /// DTO used for user authentication during login.
    /// Contains the user’s credentials for obtaining a token.
    /// </summary>
    public class LoginDto
    {
        /// <summary>
        /// Gets the registered email address of the user.
        /// </summary>
        /// <example>user@example.com</example>
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Email must be a valid email address.")]
        public string Email { get; init; } = string.Empty;

        /// <summary>
        /// Gets the password of the user.
        /// </summary>
        /// <remarks>
        /// Must be at least 6 characters long.
        /// </remarks>
        /// <example>P@ssw0rd!</example>
        [Required(ErrorMessage = "Password is required.")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
        public string Password { get; init; } = string.Empty;
    }
}