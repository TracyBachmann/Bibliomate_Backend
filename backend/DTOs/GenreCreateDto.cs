namespace backend.DTOs
{
    /// <summary>
    /// DTO used to create or update a genre.
    /// </summary>
    public class GenreCreateDto
    {
        public string Name { get; set; } = string.Empty;
    }
}