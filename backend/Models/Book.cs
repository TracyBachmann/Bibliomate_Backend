using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class Book
    {
        [Key]
        public int BookId { get; set; }

        [Required]
        [MaxLength(255)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(13)]
        public string Isbn { get; set; } = string.Empty;

        [Required]
        public DateTime PublicationDate { get; set; }

        public int AuthorId { get; set; }
        [ForeignKey(nameof(AuthorId))]
        public Author Author { get; set; } = null!;

        public int GenreId { get; set; }
        [ForeignKey(nameof(GenreId))]
        public Genre Genre { get; set; } = null!;

        public int EditorId { get; set; }
        [ForeignKey(nameof(EditorId))]
        public Editor Editor { get; set; } = null!;

        [Required]
        public int ShelfLevelId { get; set; }
        [ForeignKey(nameof(ShelfLevelId))]
        public ShelfLevel ShelfLevel { get; set; } = null!;
        
        public string? CoverUrl { get; set; }

        public ICollection<Loan> Loans { get; set; } = new List<Loan>();
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
        public ICollection<BookTag> BookTags { get; set; } = new List<BookTag>();
        public Stock? Stock { get; set; }
    }
}