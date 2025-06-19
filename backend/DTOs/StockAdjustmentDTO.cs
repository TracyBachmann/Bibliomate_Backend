namespace backend.Models.Dtos
{
    public class StockAdjustmentDto
    {
        /// <summary>
        /// Quantity to adjust (positive to add, negative to remove).
        /// </summary>
        public int Adjustment { get; set; }
    }
}