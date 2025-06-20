namespace backend.Models.DTOs
{
    /// <summary>
    /// DTO used to initiate a password reset request.
    /// </summary>
    public class RequestPasswordResetDTO
    {
        public string Email { get; set; } = string.Empty;
    }
}