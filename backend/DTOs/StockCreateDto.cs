namespace backend.DTOs
{
    /// <summary>
    /// DTO used to initialize stock for a book.
    /// </summary>
    public class StockCreateDto
    {
        public int BookId { get; set; }
        public int Quantity { get; set; }
    }
}