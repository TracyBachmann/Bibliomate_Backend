namespace backend.DTOs
{
    public class CreateUserDTO
    {
        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string? Address { get; set; }

        public string? Phone { get; set; }
    }
}