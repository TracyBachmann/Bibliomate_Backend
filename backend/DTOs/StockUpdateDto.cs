using System.ComponentModel.DataAnnotations;

namespace backend.DTOs
{
    /// <summary>
    /// DTO used to update existing stock information.
    /// </summary>
    public class StockUpdateDto
    {
        /// <summary>
        /// Unique identifier of the stock entry to update.
        /// </summary>
        /// <example>15</example>
        [Required(ErrorMessage = "StockId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "StockId must be a positive integer.")]
        public int StockId { get; set; }

        /// <summary>
        /// Identifier of the book associated with this stock entry.
        /// </summary>
        /// <example>42</example>
        [Required(ErrorMessage = "BookId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "BookId must be a positive integer.")]
        public int BookId { get; set; }

        /// <summary>
        /// Updated quantity available in stock.
        /// </summary>
        /// <example>10</example>
        [Required(ErrorMessage = "Quantity is required.")]
        [Range(0, int.MaxValue, ErrorMessage = "Quantity must be zero or a positive integer.")]
        public int Quantity { get; set; }

        /// <summary>
        /// Indicates whether at least one copy is currently available.
        /// </summary>
        /// <example>true</example>
        [Required(ErrorMessage = "IsAvailable is required.")]
        public bool IsAvailable { get; set; }
    }
}