namespace backend.DTOs
{
    /// <summary>
    /// DTO used to update a user's role.
    /// </summary>
    public class UserRoleUpdateDto
    {
        public string Role { get; set; } = string.Empty;
    }
}