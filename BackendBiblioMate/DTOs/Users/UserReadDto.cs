namespace BackendBiblioMate.DTOs
{
    /// <summary>
    /// DTO returned when retrieving user account information.
    /// Contains identifier, personal details, and assigned role.
    /// </summary>
    public class UserReadDto
    {
        /// <summary>
        /// Gets the unique identifier of the user.
        /// </summary>
        /// <example>7</example>
        public int UserId { get; init; }

        /// <summary>
        /// Gets the full name of the user.
        /// </summary>
        /// <example>Jane Doe</example>
        public string Name { get; init; } = string.Empty;

        /// <summary>
        /// Gets the email address of the user.
        /// </summary>
        /// <example>jane.doe@example.com</example>
        public string Email { get; init; } = string.Empty;

        /// <summary>
        /// Gets the role assigned to the user.
        /// </summary>
        /// <remarks>
        /// Possible values: User, Librarian, Admin.
        /// </remarks>
        /// <example>User</example>
        public string Role { get; init; } = string.Empty;
    }
}