using System.ComponentModel.DataAnnotations;

namespace backend.DTOs
{
    /// <summary>
    /// DTO used to update a user's role.
    /// </summary>
    public class UserRoleUpdateDto
    {
        /// <summary>
        /// New role to assign to the user (e.g., "User", "Librarian", or "Admin").
        /// </summary>
        /// <example>Admin</example>
        [Required(ErrorMessage = "Role is required.")]
        [RegularExpression("^(User|Librarian|Admin)$", ErrorMessage = "Role must be one of the following values: User, Librarian, Admin.")]
        public string Role { get; set; } = string.Empty;
    }
}