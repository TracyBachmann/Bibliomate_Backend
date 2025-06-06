using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class Stock
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int BookId { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Quantity cannot be negative.")]
        public int Quantity { get; set; }

        public bool IsAvailable { get; set; }

        [ForeignKey("BookId")]
        public Book Book { get; set; } = null!;
    }
}