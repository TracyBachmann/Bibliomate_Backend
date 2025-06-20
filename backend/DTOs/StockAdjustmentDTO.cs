namespace backend.Models.DTOs
{
    /// <summary>
    /// DTO used to increase or decrease book stock.
    /// </summary>
    public class StockAdjustmentDTO
    {
        public int Adjustment { get; set; }
    }
}