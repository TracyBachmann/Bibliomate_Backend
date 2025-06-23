using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    /// <summary>
    /// Represents the stock information for a specific book (physical copies).
    /// </summary>
    public class Stock
    {
        /// <summary>
        /// Primary key of the stock entry.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int StockId { get; set; }

        /// <summary>
        /// Identifier of the book associated with this stock.
        /// </summary>
        [Required(ErrorMessage = "BookId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "BookId must be a positive integer.")]
        public int BookId { get; set; }

        /// <summary>
        /// Number of available copies of the book.
        /// </summary>
        [Required(ErrorMessage = "Quantity is required.")]
        [Range(0, int.MaxValue, ErrorMessage = "Quantity cannot be negative.")]
        public int Quantity { get; set; }

        /// <summary>
        /// Indicates if at least one copy is available for loan.
        /// </summary>
        public bool IsAvailable { get; set; }

        /// <summary>
        /// Navigation property for the related book.
        /// </summary>
        [ForeignKey(nameof(BookId))]
        public Book Book { get; set; } = null!;
    }
}