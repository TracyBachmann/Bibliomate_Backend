using System.ComponentModel.DataAnnotations;

namespace backend.DTOs
{
    /// <summary>
    /// DTO used to update user personal information.
    /// </summary>
    public class UserUpdateDto
    {
        /// <summary>
        /// Updated full name of the user.
        /// </summary>
        /// <example>Jane Doe</example>
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters.")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Updated email address of the user.
        /// </summary>
        /// <example>jane.doe@example.com</example>
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Email must be a valid email address.")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Updated phone number of the user.
        /// </summary>
        /// <example>+33 6 12 34 56 78</example>
        [Required(ErrorMessage = "Phone is required.")]
        [Phone(ErrorMessage = "Phone must be a valid phone number.")]
        public string Phone { get; set; } = string.Empty;

        /// <summary>
        /// Updated postal address of the user.
        /// </summary>
        /// <example>123 Main St, Springfield</example>
        [Required(ErrorMessage = "Address is required.")]
        [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters.")]
        public string Address { get; set; } = string.Empty;
    }
}