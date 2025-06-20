namespace backend.DTOs
{
    /// <summary>
    /// DTO used to reset a user's password with a token.
    /// </summary>
    public class ResetPasswordDto
    {
        public string Token { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}