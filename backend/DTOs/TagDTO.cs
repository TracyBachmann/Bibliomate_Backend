namespace backend.Models.DTOs
{
    /// <summary>
    /// DTO representing a tag.
    /// </summary>
    public class TagDTO
    {
        public int TagId { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}