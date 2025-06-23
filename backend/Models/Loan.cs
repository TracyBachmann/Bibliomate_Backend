using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    /// <summary>
    /// Represents a book loan made by a user.
    /// </summary>
    public class Loan
    {
        /// <summary>
        /// Primary key of the loan record.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LoanId { get; set; }

        /// <summary>
        /// Identifier of the book being loaned.
        /// </summary>
        [Required(ErrorMessage = "BookId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "BookId must be a positive integer.")]
        public int BookId { get; set; }

        /// <summary>
        /// Identifier of the user who borrowed the book.
        /// </summary>
        [Required(ErrorMessage = "UserId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "UserId must be a positive integer.")]
        public int UserId { get; set; }

        /// <summary>
        /// Date and time when the loan was created.
        /// </summary>
        [Required(ErrorMessage = "LoanDate is required.")]
        public DateTime LoanDate { get; set; }

        /// <summary>
        /// Date and time when the book is due to be returned.
        /// </summary>
        [Required(ErrorMessage = "DueDate is required.")]
        public DateTime DueDate { get; set; }

        /// <summary>
        /// Date and time when the book was actually returned; null if not yet returned.
        /// </summary>
        public DateTime? ReturnDate { get; set; }

        /// <summary>
        /// Fine amount applied for a late return. Must be zero or positive.
        /// </summary>
        [Column(TypeName = "decimal(10,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "Fine must be zero or positive.")]
        public decimal Fine { get; set; } = 0m;

        /// <summary>
        /// Navigation property for the related book.
        /// </summary>
        [ForeignKey(nameof(BookId))]
        public Book Book { get; set; } = null!;

        /// <summary>
        /// Navigation property for the user who borrowed the book.
        /// </summary>
        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;

        /// <summary>
        /// Identifier of the specific stock entry (copy) loaned.
        /// </summary>
        [Required(ErrorMessage = "StockId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "StockId must be a positive integer.")]
        public int StockId { get; set; }

        /// <summary>
        /// Navigation property for the stock entry representing the physical copy.
        /// </summary>
        [ForeignKey(nameof(StockId))]
        public Stock Stock { get; set; } = null!;
    }
}
