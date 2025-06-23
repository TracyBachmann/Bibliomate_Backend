using System.ComponentModel.DataAnnotations;

namespace backend.DTOs
{
    /// <summary>
    /// DTO used to initialize stock for a book.
    /// </summary>
    public class StockCreateDto
    {
        /// <summary>
        /// Identifier of the book for which stock is being created.
        /// </summary>
        /// <example>42</example>
        [Required(ErrorMessage = "BookId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "BookId must be a positive integer.")]
        public int BookId { get; set; }

        /// <summary>
        /// Initial quantity of the book to add to stock.
        /// </summary>
        /// <example>10</example>
        [Required(ErrorMessage = "Quantity is required.")]
        [Range(0, int.MaxValue, ErrorMessage = "Quantity must be zero or a positive integer.")]
        public int Quantity { get; set; }
    }
}