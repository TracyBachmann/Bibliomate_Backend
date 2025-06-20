using System.ComponentModel.DataAnnotations;

namespace backend.DTOs
{
    public class BookUpdateDTO
    {
        [Required]
        public int BookId { get; set; }

        [Required]
        [StringLength(255)]
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