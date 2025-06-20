namespace backend.DTOs
{
    /// <summary>
    /// DTO used to read notification information.
    /// </summary>
    public class NotificationReadDto
    {
        public int NotificationId { get; set; }

        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}