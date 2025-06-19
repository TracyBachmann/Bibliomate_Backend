using backend.DTOs;

public class BookReadDto
{
    public int BookId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Isbn { get; set; } = string.Empty;
    public int PublicationYear { get; set; }

    public string AuthorName { get; set; } = string.Empty;
    public string GenreName { get; set; } = string.Empty;
    public string EditorName { get; set; } = string.Empty;

    public List<TagDto> Tags { get; set; } = new();
}