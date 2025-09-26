using System.ComponentModel.DataAnnotations;

namespace BackendBiblioMate.DTOs
{
    /// <summary>
    /// Data Transfer Object used to update existing stock information.
    /// Contains all fields that can be modified on a stock record.
    /// </summary>
    public class StockUpdateDto
    {
        /// <summary>
        /// Gets or sets the unique identifier of the stock entry to update.
        /// </summary>
        /// <example>15</example>
        [Required(ErrorMessage = "StockId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "StockId must be a positive integer.")]
        public int StockId { get; init; }

        /// <summary>
        /// Gets or sets the identifier of the book associated with this stock entry.
        /// </summary>
        /// <example>42</example>
        [Required(ErrorMessage = "BookId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "BookId must be a positive integer.")]
        public int BookId { get; init; }

        /// <summary>
        /// Gets or sets the updated quantity available in stock.
        /// </summary>
        /// <remarks>
        /// Must be zero or a positive integer.
        /// </remarks>
        /// <example>10</example>
        [Required(ErrorMessage = "Quantity is required.")]
        [Range(0, int.MaxValue, ErrorMessage = "Quantity must be zero or a positive integer.")]
        public int Quantity { get; init; }

        /// <summary>
        /// Gets or sets a value indicating whether at least one copy is currently available.
        /// </summary>
        /// <example>true</example>
        [Required(ErrorMessage = "IsAvailable is required.")]
        public bool IsAvailable { get; init; }
    }
}