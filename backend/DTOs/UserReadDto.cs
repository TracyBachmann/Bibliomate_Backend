namespace backend.DTOs
{
    /// <summary>
    /// DTO returned when retrieving user account information.
    /// </summary>
    public class UserReadDto
    {
        /// <summary>
        /// Unique identifier of the user.
        /// </summary>
        /// <example>7</example>
        public int UserId { get; set; }

        /// <summary>
        /// Full name of the user.
        /// </summary>
        /// <example>Jane Doe</example>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Email address of the user.
        /// </summary>
        /// <example>jane.doe@example.com</example>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Role assigned to the user (e.g., User, Librarian, Admin).
        /// </summary>
        /// <example>User</example>
        public string Role { get; set; } = string.Empty;
    }
}