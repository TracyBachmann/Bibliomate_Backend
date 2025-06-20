namespace backend.DTOs
{
    /// <summary>
    /// DTO used to read genre information.
    /// </summary>
    public class GenreReadDto
    {
        public int GenreId { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}