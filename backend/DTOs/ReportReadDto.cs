namespace backend.DTOs
{
    /// <summary>
    /// DTO returned when retrieving report information.
    /// </summary>
    public class ReportReadDto
    {
        /// <summary>
        /// Unique identifier of the report.
        /// </summary>
        /// <example>8</example>
        public int ReportId { get; set; }

        /// <summary>
        /// Title of the report.
        /// </summary>
        /// <example>Monthly Usage Statistics</example>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Detailed content of the report.
        /// </summary>
        /// <example>The number of loans increased by 15% compared to the previous month...</example>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Date and time when the report was generated.
        /// </summary>
        /// <example>2025-06-20T14:30:00Z</example>
        public DateTime GeneratedDate { get; set; }

        /// <summary>
        /// Identifier of the user who generated the report.
        /// </summary>
        /// <example>7</example>
        public int UserId { get; set; }

        /// <summary>
        /// Full name of the user who generated the report.
        /// </summary>
        /// <example>Jane Doe</example>
        public string UserName { get; set; } = string.Empty;
    }
}