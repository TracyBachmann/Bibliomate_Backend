namespace backend.Models.DTOs
{
    /// <summary>
    /// DTO used to update a user's role.
    /// </summary>
    public class UpdateUserRoleDTO
    {
        public string Role { get; set; } = string.Empty;
    }
}