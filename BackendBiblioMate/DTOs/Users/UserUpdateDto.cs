using System.ComponentModel.DataAnnotations;

namespace BackendBiblioMate.DTOs
{
    /// <summary>
    /// Data Transfer Object used to update an existing user's personal details.
    /// Contains editable fields such as name, email, address, phone, and date of birth.
    /// </summary>
    public class UserUpdateDto
    {
        /// <summary>
        /// Gets or sets the given name of the user.
        /// </summary>
        /// <example>Jane</example>
        [Required(ErrorMessage = "First name is required.")]
        [MinLength(2, ErrorMessage = "First name must be at least 2 characters long.")]
        [MaxLength(100, ErrorMessage = "First name cannot exceed 100 characters.")]
        public string FirstName { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the family name of the user.
        /// </summary>
        /// <example>Doe</example>
        [Required(ErrorMessage = "Last name is required.")]
        [MinLength(2, ErrorMessage = "Last name must be at least 2 characters long.")]
        [MaxLength(100, ErrorMessage = "Last name cannot exceed 100 characters.")]
        public string LastName { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the email address of the user.
        /// </summary>
        /// <example>jane.doe@example.com</example>
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Email must be a valid email address.")]
        public string Email { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the phone number of the user (optional).
        /// </summary>
        /// <example>+33 6 12 34 56 78</example>
        [Phone(ErrorMessage = "Phone must be a valid phone number.")]
        public string? Phone { get; init; }

        /// <summary>
        /// Gets or sets the primary address line of the user (optional).
        /// </summary>
        /// <example>123 Main St</example>
        [MaxLength(200, ErrorMessage = "Address1 cannot exceed 200 characters.")]
        public string? Address1 { get; init; }

        /// <summary>
        /// Gets or sets the secondary address line of the user, if any (optional).
        /// </summary>
        /// <example>Apartment 4B</example>
        [MaxLength(200, ErrorMessage = "Address2 cannot exceed 200 characters.")]
        public string? Address2 { get; init; }

        /// <summary>
        /// Gets or sets the date of birth of the user (optional).
        /// </summary>
        /// <example>1995-04-21</example>
        public DateTime? DateOfBirth { get; init; }
    }
}