using System.ComponentModel.DataAnnotations;
using BackendBiblioMate.Models.Enums;

namespace BackendBiblioMate.DTOs
{
    /// <summary>
    /// Data Transfer Object used by administrators or librarians to manually create a new user account.
    /// Contains all fields required for user creation, with optional address and phone.
    /// </summary>
    public class UserCreateDto
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
        /// Gets or sets the email address for the new user.
        /// </summary>
        /// <example>jane.doe@example.com</example>
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Email must be a valid email address.")]
        public string Email { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the initial password for the new user account.
        /// </summary>
        /// <remarks>
        /// Must be between 6 and 100 characters.
        /// </remarks>
        /// <example>P@ssw0rd!</example>
        [Required(ErrorMessage = "Password is required.")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
        [MaxLength(100, ErrorMessage = "Password cannot exceed 100 characters.")]
        public string Password { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the primary address line of the user (optional).
        /// </summary>
        /// <remarks>
        /// Maximum length of 200 characters.
        /// </remarks>
        /// <example>123 Main St</example>
        [MaxLength(200, ErrorMessage = "Address1 cannot exceed 200 characters.")]
        public string? Address1 { get; init; }

        /// <summary>
        /// Gets or sets the secondary address line of the user, if any (optional).
        /// </summary>
        /// <remarks>
        /// Maximum length of 200 characters.
        /// </remarks>
        /// <example>Apartment 4B</example>
        [MaxLength(200, ErrorMessage = "Address2 cannot exceed 200 characters.")]
        public string? Address2 { get; init; }

        /// <summary>
        /// Gets or sets the phone number of the user (optional).
        /// </summary>
        /// <remarks>
        /// Must be a valid phone number.
        /// </remarks>
        /// <example>+33 6 12 34 56 78</example>
        [Phone(ErrorMessage = "Phone must be a valid phone number.")]
        public string? Phone { get; init; }

        /// <summary>
        /// Gets or sets the date of birth of the user (optional).
        /// </summary>
        /// <example>1995-04-21</example>
        public DateTime? DateOfBirth { get; init; }

        /// <summary>
        /// Gets or sets the role assigned to the new user.
        /// </summary>
        /// <remarks>
        /// Must be one of the defined roles: User, Librarian, Admin.
        /// </remarks>
        /// <example>User</example>
        [Required(ErrorMessage = "Role is required.")]
        [RegularExpression("^(User|Librarian|Admin)$", ErrorMessage = "Role must be one of: User, Librarian, Admin.")]
        public string Role { get; init; } = UserRoles.User;
    }
}