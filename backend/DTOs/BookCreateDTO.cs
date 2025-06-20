using System.ComponentModel.DataAnnotations;

namespace backend.Models.DTOs
{
    /// <summary>
    /// DTO used to create a new book entry.
    /// </summary>
    public class BookCreateDTO
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(13, MinimumLength = 10)]
        public string Isbn { get; set; } = string.Empty;

        [Required]
        public DateTime PublicationDate { get; set; }

        [Required]
        public int AuthorId { get; set; }

        [Required]
        public int GenreId { get; set; }

        [Required]
        public int EditorId { get; set; }

        [Required]
        public int ShelfLevelId { get; set; }

        public List<int>? TagIds { get; set; }
    }
}