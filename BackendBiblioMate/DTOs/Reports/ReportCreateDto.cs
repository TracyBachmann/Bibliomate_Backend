using System.ComponentModel.DataAnnotations;

namespace BackendBiblioMate.DTOs
{
    /// <summary>
    /// Data Transfer Object used to request the creation of a new analytical report.
    /// Only the <see cref="Title"/> is provided by the client; the <c>Content</c> is generated on the server.
    /// </summary>
    public class ReportCreateDto
    {
        /// <summary>
        /// Gets or sets the title for the new report.
        /// </summary>
        /// <remarks>
        /// Must contain between 1 and 200 characters.
        /// </remarks>
        /// <example>Monthly Loan Statistics</example>
        [Required(ErrorMessage = "Title is required.")]
        [MinLength(1, ErrorMessage = "Title must be at least 1 character long.")]
        [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
        public string Title { get; init; } = string.Empty;
    }
}