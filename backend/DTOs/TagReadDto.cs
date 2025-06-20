namespace backend.DTOs
{
    /// <summary>
    /// DTO used to read tag data.
    /// </summary>
    public class TagReadDto
    {
        public int TagId { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}