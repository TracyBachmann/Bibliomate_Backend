using System.ComponentModel.DataAnnotations;

namespace BackendBiblioMate.DTOs
{
    /// <summary>
    /// Data Transfer Object used to update a user's role.
    /// Contains the new role assignment for an existing user.
    /// </summary>
    public class UserRoleUpdateDto
    {
        /// <summary>
        /// Gets or sets the new role to assign to the user.
        /// </summary>
        /// <remarks>
        /// Must be one of the defined roles: User, Librarian, Admin.
        /// </remarks>
        /// <example>Admin</example>
        [Required(ErrorMessage = "Role is required.")]
        [RegularExpression("^(User|Librarian|Admin)$", ErrorMessage = "Role must be one of the following values: User, Librarian, Admin.")]
        public string Role { get; init; } = string.Empty;
    }
}