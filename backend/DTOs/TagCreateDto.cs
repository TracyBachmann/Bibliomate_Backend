namespace backend.DTOs
{
    /// <summary>
    /// DTO representing a tag.
    /// </summary>
    public class TagCreateDto
    {
        public int TagId { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}