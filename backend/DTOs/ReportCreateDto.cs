using System.ComponentModel.DataAnnotations;

namespace backend.DTOs
{
    /// <summary>
    /// DTO used to create a new report.
    /// </summary>
    public class ReportCreateDto
    {
        /// <summary>
        /// Title of the report.
        /// </summary>
        /// <example>Monthly Usage Statistics</example>
        [Required(ErrorMessage = "Title is required.")]
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters.")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Detailed content of the report.
        /// </summary>
        /// <example>The number of loans increased by 15% compared to the previous month...</example>
        [Required(ErrorMessage = "Content is required.")]
        [StringLength(1000, ErrorMessage = "Content cannot exceed 1000 characters.")]
        public string Content { get; set; } = string.Empty;
    }
}