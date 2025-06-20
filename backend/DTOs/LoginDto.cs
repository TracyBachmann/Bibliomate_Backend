namespace backend.DTOs
{
    /// <summary>
    /// DTO used for user authentication during login.
    /// </summary>
    public class LoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}