namespace backend.DTOs
{
    /// <summary>
    /// DTO used to initiate a password reset request.
    /// </summary>
    public class RequestPasswordResetDto
    {
        public string Email { get; set; } = string.Empty;
    }
}