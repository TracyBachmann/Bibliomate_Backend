using System.ComponentModel.DataAnnotations;

namespace backend.DTOs
{
    public class BookReadDto
    {
        public int BookId { get; set; }

        [Required]
        [StringLength(255)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(13, MinimumLength = 10)]
        public string Isbn { get; set; } = string.Empty;

        [Range(0, 2100)]
        public int PublicationYear { get; set; }

        [Required]
        [StringLength(100)]
        public string AuthorName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string GenreName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string EditorName { get; set; } = string.Empty;

        public bool IsAvailable { get; set; }

        public List<string> Tags { get; set; } = new();
    }
}