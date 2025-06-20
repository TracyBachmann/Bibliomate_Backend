namespace backend.Models.DTOs
{
    /// <summary>
    /// DTO used to update user personal information.
    /// </summary>
    public class UpdateUserDTO
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }
}