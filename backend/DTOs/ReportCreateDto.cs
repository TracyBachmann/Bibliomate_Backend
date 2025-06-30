namespace backend.DTOs
{
    /// <summary>
    /// Client payload to request a new analytical report.
    /// Only a Title is required; the Content is generated on the server.
    /// </summary>
    public class ReportCreateDto
    {
        /// <summary>
        /// Title for the new report.
        /// </summary>
        public string Title { get; set; } = string.Empty;
    }
}