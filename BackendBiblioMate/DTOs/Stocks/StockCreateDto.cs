using System.ComponentModel.DataAnnotations;

namespace BackendBiblioMate.DTOs
{
    /// <summary>
    /// Data Transfer Object used to initialize stock for a book.
    /// Contains the book identifier and the initial quantity to set.
    /// </summary>
    public class StockCreateDto
    {
        /// <summary>
        /// Gets or sets the identifier of the book for which stock is being created.
        /// </summary>
        /// <example>42</example>
        [Required(ErrorMessage = "BookId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "BookId must be a positive integer.")]
        public int BookId { get; init; }

        /// <summary>
        /// Gets or sets the initial quantity of the book to add to stock.
        /// </summary>
        /// <remarks>
        /// Must be zero or a positive integer.
        /// </remarks>
        /// <example>10</example>
        [Required(ErrorMessage = "Quantity is required.")]
        [Range(0, int.MaxValue, ErrorMessage = "Quantity must be zero or a positive integer.")]
        public int Quantity { get; init; }
    }
}