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
        /// Gets the unique identifier of the stock entry.
        /// </summary>
        /// <example>15</example>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int StockId { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the associated book.
        /// </summary>
        /// <example>42</example>
        [Range(1, int.MaxValue, ErrorMessage = "BookId must be a positive integer.")]
        public int BookId { get; set; }

        /// <summary>
        /// Gets or sets the current number of copies available for this book.
        /// </summary>
        /// <remarks>
        /// Must be zero or a positive integer.
        /// </remarks>
        /// <example>7</example>
        [Range(0, int.MaxValue, ErrorMessage = "Quantity must be zero or a positive integer.")]
        public int Quantity { get; set; }

        /// <summary>
        /// Gets a value indicating whether at least one copy is available for loan.
        /// </summary>
        /// <remarks>
        /// This is a computed property and is not mapped to the database.
        /// </remarks>
        /// <example>true</example>
        [NotMapped]
        public bool IsAvailable { get; set; }

        /// <summary>
        /// Navigation property for the related book.
        /// </summary>
        [ForeignKey(nameof(BookId))]
        public Book Book { get; set; } = null!;

        /// <summary>
        /// Gets the collection of loan records associated with this stock entry.
        /// </summary>
        public ICollection<Loan> Loans { get; set; } = new List<Loan>();
    }
}
