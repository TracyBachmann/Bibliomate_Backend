using System.ComponentModel.DataAnnotations;

namespace BackendBiblioMate.DTOs
{
    /// <summary>
    /// Data Transfer Object used to update an existing report.
    /// Contains the fields that can be modified on a report record.
    /// </summary>
    public class ReportUpdateDto
    {
        /// <summary>
        /// Gets or sets the unique identifier of the report to update.
        /// </summary>
        /// <example>8</example>
        [Required(ErrorMessage = "ReportId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "ReportId must be a positive integer.")]
        public int ReportId { get; init; }

        /// <summary>
        /// Gets or sets the updated title of the report.
        /// </summary>
        /// <remarks>
        /// Must contain between 1 and 200 characters.
        /// </remarks>
        /// <example>Monthly Usage Summary</example>
        [Required(ErrorMessage = "Title is required.")]
        [MinLength(1, ErrorMessage = "Title must be at least 1 character long.")]
        [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
        public string Title { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the updated detailed content of the report.
        /// </summary>
        /// <remarks>
        /// Must contain between 1 and 4000 characters.
        /// </remarks>
        /// <example>The number of loans increased by 20% compared to the previous month...</example>
        [Required(ErrorMessage = "Content is required.")]
        [MinLength(1, ErrorMessage = "Content must be at least 1 character long.")]
        [MaxLength(4000, ErrorMessage = "Content cannot exceed 4000 characters.")]
        public string Content { get; init; } = string.Empty;
    }
}