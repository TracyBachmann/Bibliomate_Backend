using System.ComponentModel.DataAnnotations;

namespace BackendBiblioMate.DTOs
{
    /// <summary>
    /// DTO used to register a new user.
    /// Contains all fields required by the multi-step signup form.
    /// </summary>
    public class RegisterDto
    {
        /// <summary>
        /// Gets the user's given name.
        /// </summary>
        /// <remarks>
        /// Must be between 2 and 60 characters.
        /// </remarks>
        /// <example>Jane</example>
        [Required(ErrorMessage = "First name is required.")]
        [MinLength(2, ErrorMessage = "First name must be at least 2 characters long.")]
        [MaxLength(60, ErrorMessage = "First name cannot exceed 60 characters.")]
        public string FirstName { get; init; } = string.Empty;

        /// <summary>
        /// Gets the user's family name.
        /// </summary>
        /// <remarks>
        /// Must be between 2 and 60 characters.
        /// </remarks>
        /// <example>Doe</example>
        [Required(ErrorMessage = "Last name is required.")]
        [MinLength(2, ErrorMessage = "Last name must be at least 2 characters long.")]
        [MaxLength(60, ErrorMessage = "Last name cannot exceed 60 characters.")]
        public string LastName { get; init; } = string.Empty;

        /// <summary>
        /// Gets the email address of the user.
        /// </summary>
        /// <example>jane.doe@example.com</example>
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Email must be a valid email address.")]
        [MaxLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
        public string Email { get; init; } = string.Empty;

        /// <summary>
        /// Gets the phone number of the user.
        /// </summary>
        /// <example>+33 6 12 34 56 78</example>
        [Required(ErrorMessage = "Phone is required.")]
        [Phone(ErrorMessage = "Phone must be a valid phone number.")]
        [MaxLength(30, ErrorMessage = "Phone cannot exceed 30 characters.")]
        public string Phone { get; init; } = string.Empty;

        /// <summary>
        /// Gets the primary address line of the user.
        /// </summary>
        /// <remarks>
        /// Maximum length of 200 characters.
        /// </remarks>
        /// <example>123 Main St</example>
        [Required(ErrorMessage = "Address line 1 is required.")]
        [MinLength(1, ErrorMessage = "Address line 1 must be at least 1 character long.")]
        [MaxLength(200, ErrorMessage = "Address line 1 cannot exceed 200 characters.")]
        public string Address1 { get; init; } = string.Empty;

        /// <summary>
        /// Gets the secondary address line of the user (optional).
        /// </summary>
        /// <remarks>
        /// Maximum length of 200 characters.
        /// </remarks>
        /// <example>Apartment 4B</example>
        [MaxLength(200, ErrorMessage = "Address line 2 cannot exceed 200 characters.")]
        public string? Address2 { get; init; }

        /// <summary>
        /// Gets the user's date of birth (optional).
        /// </summary>
        /// <example>1995-04-21</example>
        public DateTime? DateOfBirth { get; init; }

        /// <summary>
        /// Gets the password for the user account.
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
        /// Gets the profile image payload or path (optional).
        /// </summary>
        /// <remarks>
        /// Can be a URL/path or a Base64-encoded string depending on the chosen storage strategy.
        /// </remarks>
        /// <example>https://cdn.example.com/u/42/profile.png</example>
        [MaxLength(2000, ErrorMessage = "Profile image value cannot exceed 2000 characters.")]
        public string? ProfileImage { get; init; }

        /// <summary>
        /// Gets the list of preferred genre identifiers (optional).
        /// </summary>
        /// <example>[1, 3, 7]</example>
        public IEnumerable<int> FavoriteGenreIds { get; init; } = Enumerable.Empty<int>();
    }
}
