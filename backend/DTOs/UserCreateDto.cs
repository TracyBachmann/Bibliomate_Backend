using System.ComponentModel.DataAnnotations;
using backend.Models.Enums;

namespace backend.DTOs
{
    /// <summary>
    /// DTO used by administrators or librarians to manually create a new user account.
    /// </summary>
    public class UserCreateDto
    {
        /// <summary>
        /// Full name of the user.
        /// </summary>
        /// <example>Jane Doe</example>
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters.")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Email address for the new user.
        /// </summary>
        /// <example>jane.doe@example.com</example>
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Email must be a valid email address.")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Initial password for the new user account.
        /// </summary>
        /// <example>P@ssw0rd!</example>
        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters.")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Postal address of the user (optional).
        /// </summary>
        /// <example>123 Main St, Springfield</example>
        [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters.")]
        public string? Address { get; set; }

        /// <summary>
        /// Phone number of the user (optional).
        /// </summary>
        /// <example>+33 6 12 34 56 78</example>
        [Phone(ErrorMessage = "Phone must be a valid phone number.")]
        public string? Phone { get; set; }

        /// <summary>
        /// Role assigned to the new user (User, Librarian, Admin).
        /// </summary>
        /// <example>User</example>
        [Required(ErrorMessage = "Role is required.")]
        [RegularExpression("^(User|Librarian|Admin)$", ErrorMessage = "Role must be one of: User, Librarian, Admin.")]
        public string Role { get; set; } = UserRoles.User;
    }
}