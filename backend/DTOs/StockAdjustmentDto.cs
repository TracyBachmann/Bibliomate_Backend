using System.ComponentModel.DataAnnotations;

namespace backend.DTOs
{
    /// <summary>
    /// DTO used to adjust the quantity of a specific stock entry.
    /// </summary>
    public class StockAdjustmentDto
    {
        /// <summary>
        /// Number of units to adjust the stock by. Positive to increase, negative to decrease.
        /// </summary>
        /// <example>-1</example>
        [Required(ErrorMessage = "Adjustment is required.")]
        [Range(-1000, 1000, ErrorMessage = "Adjustment must be between -1000 and 1000.")]
        public int Adjustment { get; set; }
    }
}