namespace backend.DTOs
{
    /// <summary>
    /// DTO used to update stock information.
    /// </summary>
    public class StockUpdateDto
    {
        public int StockId { get; set; }
        public int BookId { get; set; }
        public int Quantity { get; set; }
        public bool IsAvailable { get; set; }
    }
}