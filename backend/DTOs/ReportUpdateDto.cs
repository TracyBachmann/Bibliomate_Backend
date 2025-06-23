using System.ComponentModel.DataAnnotations;

namespace backend.DTOs
{
    /// <summary>
    /// DTO used to update an existing report.
    /// </summary>
    public class ReportUpdateDto
    {
        /// <summary>
        /// Unique identifier of the report to update.
        /// </summary>
        /// <example>8</example>
        [Required(ErrorMessage = "ReportId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "ReportId must be a positive integer.")]
        public int ReportId { get; set; }

        /// <summary>
        /// Updated title of the report.
        /// </summary>
        /// <example>Monthly Usage Summary</example>
        [Required(ErrorMessage = "Title is required.")]
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters.")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Updated detailed content of the report.
        /// </summary>
        /// <example>The number of loans increased by 20% compared to the previous month...</example>
        [Required(ErrorMessage = "Content is required.")]
        [StringLength(1000, ErrorMessage = "Content cannot exceed 1000 characters.")]
        public string Content { get; set; } = string.Empty;
    }
}