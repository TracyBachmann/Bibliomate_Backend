namespace backend.Models.DTOs
{
    /// <summary>
    /// DTO representing a notification to be sent to users.
    /// </summary>
    public class NotificationDTO
    {
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = "info"; // info, warning, error, etc.
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }
}