using System.ComponentModel.DataAnnotations;
using BackendBiblioMate.Models.Enums;

namespace BackendBiblioMate.DTOs
{
    /// <summary>
    /// DTO used by administrators or librarians to manually create a new user account.
    /// Contains all fields required for user creation, with optional address and phone.
    /// </summary>
    public class UserCreateDto
    {
        /// <summary>
        /// Gets the full name of the user.
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
        /// Gets the email address for the new user.
        /// </summary>
        /// <example>jane.doe@example.com</example>
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Email must be a valid email address.")]
        public string Email { get; init; } = string.Empty;

        /// <summary>
        /// Gets the initial password for the new user account.
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
        /// Gets the postal address of the user.
        /// </summary>
        /// <remarks>
        /// Optional. Maximum length of 200 characters.
        /// </remarks>
        /// <example>123 Main St, Springfield</example>
        [MinLength(1, ErrorMessage = "Address must be at least 1 character long.")]
        [MaxLength(200, ErrorMessage = "Address cannot exceed 200 characters.")]
        public string? Address { get; init; }

        /// <summary>
        /// Gets the phone number of the user.
        /// </summary>
        /// <remarks>
        /// Optional. Must be a valid phone number.
        /// </remarks>
        /// <example>+33 6 12 34 56 78</example>
        [Phone(ErrorMessage = "Phone must be a valid phone number.")]
        public string? Phone { get; init; }

        /// <summary>
        /// Gets the role assigned to the new user.
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