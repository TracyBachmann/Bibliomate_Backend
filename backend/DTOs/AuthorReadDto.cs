namespace backend.DTOs
{
    /// <summary>
    /// DTO used to read author information.
    /// </summary>
    public class AuthorReadDto
    {
        public int AuthorId { get; set; }

        public string Name { get; set; } = string.Empty;
    }
}