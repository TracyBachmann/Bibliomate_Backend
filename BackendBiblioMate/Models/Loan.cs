using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendBiblioMate.Models
{
    /// <summary>
    /// Represents a loan of a book by a user, including dates and any applicable penalties.
    /// </summary>
    public class Loan
    {
        /// <summary>
        /// Gets or sets the primary key of the loan record.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LoanId { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the book being loaned.
        /// </summary>
        [Required(ErrorMessage = "BookId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "BookId must be a positive integer.")]
        public int BookId { get; set; }

        /// <summary>
        /// Gets or sets the navigation property for the related book.
        /// </summary>
        [ForeignKey(nameof(BookId))]
        public Book Book { get; set; } = null!;

        /// <summary>
        /// Gets or sets the identifier of the user who borrowed the book.
        /// </summary>
        [Required(ErrorMessage = "UserId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "UserId must be a positive integer.")]
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the navigation property for the user who borrowed the book.
        /// </summary>
        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;

        /// <summary>
        /// Gets or sets the date and time when the loan was created.
        /// </summary>
        [Required(ErrorMessage = "LoanDate is required.")]
        public DateTime LoanDate { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the book is due to be returned.
        /// </summary>
        [Required(ErrorMessage = "DueDate is required.")]
        public DateTime DueDate { get; set; }  // setter ajouté

        /// <summary>
        /// Gets or sets the date and time when the book was actually returned; null if not yet returned.
        /// </summary>
        public DateTime? ReturnDate { get; set; }  // setter ajouté

        /// <summary>
        /// Gets or sets the fine amount applied for a late return. Must be zero or positive.
        /// </summary>
        [Column(TypeName = "decimal(10,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "Fine must be zero or positive.")]
        public decimal Fine { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the specific stock entry (copy) loaned.
        /// </summary>
        [Required(ErrorMessage = "StockId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "StockId must be a positive integer.")]
        public int StockId { get; set; }

        /// <summary>
        /// Gets or sets the navigation property for the stock entry representing the physical copy.
        /// </summary>
        [ForeignKey(nameof(StockId))]
        public Stock Stock { get; set; } = null!;
    }
}