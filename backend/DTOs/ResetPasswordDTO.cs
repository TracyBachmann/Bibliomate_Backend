namespace backend.Models.DTOs
{
    /// <summary>
    /// DTO used to reset a user's password with a token.
    /// </summary>
    public class ResetPasswordDTO
    {
        public string Token { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}