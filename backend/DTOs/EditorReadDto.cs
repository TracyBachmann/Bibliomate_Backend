namespace backend.DTOs
{
    /// <summary>
    /// DTO used to return editor data to clients.
    /// </summary>
    public class EditorReadDto
    {
        public int EditorId { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}