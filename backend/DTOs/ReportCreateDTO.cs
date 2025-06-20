namespace backend.Models.DTOs
{
    /// <summary>
    /// DTO used to create a report.
    /// </summary>
    public class ReportCreateDTO
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }
}