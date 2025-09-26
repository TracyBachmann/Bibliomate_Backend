using System.ComponentModel.DataAnnotations;

namespace BackendBiblioMate.DTOs
{
    /// <summary>
    /// Data Transfer Object used to adjust the quantity of a specific stock entry.
    /// Contains the delta to apply to the current stock level.
    /// </summary>
    public class StockAdjustmentDto
    {
        /// <summary>
        /// Gets or sets the number of units to adjust the stock by.
        /// </summary>
        /// <remarks>
        /// Positive values increase stock; negative values decrease stock.  
        /// A value of <c>0</c> is not allowed.
        /// </remarks>
        /// <example>-1</example>
        [Required(ErrorMessage = "Adjustment is required.")]
        [Range(-1000, 1000, ErrorMessage = "Adjustment must be between -1000 and 1000.")]
        public int Adjustment { get; init; }
    }
}