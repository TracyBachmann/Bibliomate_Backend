using System.ComponentModel.DataAnnotations;

namespace BackendBiblioMate.DTOs
{
    /// <summary>
    /// DTO used to update an existing report.
    /// Contains the fields that can be modified on a report record.
    /// </summary>
    public class ReportUpdateDto
    {
        /// <summary>
        /// Gets the unique identifier of the report to update.
        /// </summary>
        /// <example>8</example>
        [Required(ErrorMessage = "ReportId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "ReportId must be a positive integer.")]
        public int ReportId { get; init; }

        /// <summary>
        /// Gets the updated title of the report.
        /// </summary>
        /// <remarks>
        /// Maximum length of 100 characters.
        /// </remarks>
        /// <example>Monthly Usage Summary</example>
        [Required(ErrorMessage = "Title is required.")]
        [MinLength(1, ErrorMessage = "Title must be at least 1 character long.")]
        [MaxLength(100, ErrorMessage = "Title cannot exceed 100 characters.")]
        public string Title { get; init; } = string.Empty;

        /// <summary>
        /// Gets the updated detailed content of the report.
        /// </summary>
        /// <remarks>
        /// Maximum length of 1000 characters.
        /// </remarks>
        /// <example>The number of loans increased by 20% compared to the previous month...</example>
        [Required(ErrorMessage = "Content is required.")]
        [MinLength(1, ErrorMessage = "Content must be at least 1 character long.")]
        [MaxLength(1000, ErrorMessage = "Content cannot exceed 1000 characters.")]
        public string Content { get; init; } = string.Empty;
    }
}