using System.ComponentModel.DataAnnotations;

namespace BackendBiblioMate.DTOs
{
    /// <summary>
    /// DTO used to update user personal information.
    /// Contains the user’s editable personal details.
    /// </summary>
    public class UserUpdateDto
    {
        /// <summary>
        /// Gets the updated full name of the user.
        /// </summary>
        /// <remarks>
        /// Must be between 2 and 100 characters.
        /// </remarks>
        /// <example>Jane Doe</example>
        [Required(ErrorMessage = "Name is required.")]
        [MinLength(2, ErrorMessage = "Name must be at least 2 characters long.")]
        [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public string Name { get; init; } = string.Empty;

        /// <summary>
        /// Gets the updated email address of the user.
        /// </summary>
        /// <example>jane.doe@example.com</example>
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Email must be a valid email address.")]
        public string Email { get; init; } = string.Empty;

        /// <summary>
        /// Gets the updated phone number of the user.
        /// </summary>
        /// <example>+33 6 12 34 56 78</example>
        [Required(ErrorMessage = "Phone is required.")]
        [Phone(ErrorMessage = "Phone must be a valid phone number.")]
        public string Phone { get; init; } = string.Empty;

        /// <summary>
        /// Gets the updated postal address of the user.
        /// </summary>
        /// <remarks>
        /// Maximum length of 200 characters.
        /// </remarks>
        /// <example>123 Main St, Springfield</example>
        [Required(ErrorMessage = "Address is required.")]
        [MinLength(1, ErrorMessage = "Address must be at least 1 character long.")]
        [MaxLength(200, ErrorMessage = "Address cannot exceed 200 characters.")]
        public string Address { get; init; } = string.Empty;
    }
}