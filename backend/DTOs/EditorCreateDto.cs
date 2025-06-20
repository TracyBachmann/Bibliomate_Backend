namespace backend.DTOs
{
    /// <summary>
    /// DTO used to create or update an editor (publisher).
    /// </summary>
    public class EditorCreateDto
    {
        public string Name { get; set; } = string.Empty;
    }
}