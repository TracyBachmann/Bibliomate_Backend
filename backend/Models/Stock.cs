using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class Stock
    {
        [Key]
        public int StockId { get; set; }

        [Required]
        public int BookId { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "La quantité ne peut pas être négative.")]
        public int Quantity { get; set; }

        public bool IsAvailable { get; set; }

        [ForeignKey("BookId")]
        public Book Book { get; set; } = null!;
    }
}