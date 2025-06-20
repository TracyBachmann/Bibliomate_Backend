namespace backend.DTOs
{
    /// <summary>
    /// DTO used to increase or decrease book stock.
    /// </summary>
    public class StockAdjustmentDto
    {
        public int Adjustment { get; set; }
    }
}