using System.ComponentModel.DataAnnotations;

namespace backend.Models.DTOs
{
    /// <summary>
    /// DTO used to return detailed book information to clients.
    /// </summary>
    public class BookReadDTO
    {
        public int BookId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Isbn { get; set; } = string.Empty;

        public int PublicationYear { get; set; }

        public string AuthorName { get; set; } = string.Empty;

        public string GenreName { get; set; } = string.Empty;

        public string EditorName { get; set; } = string.Empty;

        public bool IsAvailable { get; set; }

        public List<string> Tags { get; set; } = new();
    }
}