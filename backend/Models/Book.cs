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
        [MaxLength(100)]
        public string Author { get; set; } = string.Empty;

        [Required]
        [MaxLength(13)]
        public string Isbn { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Publisher { get; set; } = string.Empty;

        [Required]
        public DateTime PublicationDate { get; set; }

        [Required]
        [MaxLength(100)]
        public string Genre { get; set; } = string.Empty;

        [Required]
        public int ShelfLevelId { get; set; }

        [ForeignKey(nameof(ShelfLevelId))]
        public ShelfLevel? ShelfLevel { get; set; }
        
        public ICollection<Loan>? Loans { get; set; }
        public ICollection<Reservation>? Reservations { get; set; }
    }
}