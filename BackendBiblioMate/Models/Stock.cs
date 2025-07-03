using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendBiblioMate.Models
{
    /// <summary>
    /// Represents the stock information for a specific book (physical copies).
    /// Tracks available quantity and related loans.
    /// </summary>
    public class Stock
    {
        /// <summary>
        /// Gets the primary key of the stock entry.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int StockId { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the book associated with this stock.
        /// </summary>
        [Required(ErrorMessage = "BookId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "BookId must be a positive integer.")]
        public int BookId { get; set; }

        /// <summary>
        /// Gets or sets the number of available copies of the book.
        /// </summary>
        [Required(ErrorMessage = "Quantity is required.")]
        [Range(0, int.MaxValue, ErrorMessage = "Quantity cannot be negative.")]
        public int Quantity { get; set; }

        /// <summary>
        /// Gets a value indicating whether at least one copy is available for loan.
        /// </summary>
        [NotMapped]
        public bool IsAvailable => Quantity > 0;

        /// <summary>
        /// Navigation property for the related book.
        /// </summary>
        [ForeignKey(nameof(BookId))]
        public Book Book { get; set; } = null!;

        /// <summary>
        /// Gets the collection of loans associated with this stock entry.
        /// </summary>
        public ICollection<Loan> Loans { get; set; } = new List<Loan>();
    }
}