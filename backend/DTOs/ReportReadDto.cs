namespace backend.DTOs
{
    /// <summary>
    /// DTO used to return report information to the client.
    /// </summary>
    public class ReportReadDto
    {
        public int ReportId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public DateTime GeneratedDate { get; set; }

        public int UserId { get; set; }

        public string UserName { get; set; } = string.Empty;
    }
}