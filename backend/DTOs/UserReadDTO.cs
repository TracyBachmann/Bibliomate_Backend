namespace backend.Models.DTOs
{
    /// <summary>
    /// DTO representing user data for display purposes.
    /// </summary>
    public class UserReadDTO
    {
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}