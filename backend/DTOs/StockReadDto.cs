namespace backend.DTOs
{
    /// <summary>
    /// DTO used to read stock details including book title.
    /// </summary>
    public class StockReadDto
    {
        public int StockId { get; set; }
        public int BookId { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public bool IsAvailable { get; set; }
    }
}