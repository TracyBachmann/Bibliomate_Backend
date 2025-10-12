namespace BackendBiblioMate.DTOs
{
    /// <summary>
    /// Data Transfer Object returned when retrieving user account information.
    /// Contains identifier, personal details, assigned role, and profile data.
    /// </summary>
    public class UserReadDto
    {
        /// <summary>
        /// Gets or sets the unique identifier of the user.
        /// </summary>
        /// <example>7</example>
        public int UserId { get; init; }

        /// <summary>
        /// Gets or sets the given name of the user.
        /// </summary>
        /// <example>Jane</example>
        public string FirstName { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the family name of the user.
        /// </summary>
        /// <example>Doe</example>
        public string LastName { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the email address of the user.
        /// </summary>
        /// <example>jane.doe@example.com</example>
        public string Email { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the role assigned to the user.
        /// </summary>
        /// <remarks>
        /// Possible values: User, Librarian, Admin.
        /// </remarks>
        /// <example>User</example>
        public string Role { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the primary address line of the user.
        /// </summary>
        /// <example>123 Main St</example>
        public string? Address1 { get; init; }

        /// <summary>
        /// Gets or sets the secondary address line of the user, if any.
        /// </summary>
        /// <example>Apartment 4B</example>
        public string? Address2 { get; init; }

        /// <summary>
        /// Gets or sets the phone number of the user.
        /// </summary>
        /// <example>+33 6 12 34 56 78</example>
        public string? Phone { get; init; }

        /// <summary>
        /// Gets or sets the date of birth of the user, if provided.
        /// </summary>
        /// <example>1995-04-21</example>
        public DateTime? DateOfBirth { get; init; }

        /// <summary>
        /// Gets or sets the profile image path or URL of the user, if provided.
        /// </summary>
        /// <example>https://cdn.example.com/u/42/profile.png</example>
        public string? ProfileImagePath { get; init; }

        /// <summary>
        /// Gets or sets the list of preferred genre identifiers for the user.
        /// </summary>
        /// <example>[1, 3, 7]</example>
        public IEnumerable<int> FavoriteGenreIds { get; init; } = Array.Empty<int>();
        
        /// <summary>
        /// Gets or sets whether the user account has been approved by an administrator.
        /// </summary>
        /// <example>true</example>
        public bool IsApproved { get; init; }

        /// <summary>
        /// Gets or sets whether the user's email has been confirmed.
        /// </summary>
        /// <example>true</example>
        public bool IsEmailConfirmed { get; init; }
    }
}
