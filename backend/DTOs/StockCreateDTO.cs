namespace backend.Models.DTOs
{
    /// <summary>
    /// DTO used to initialize stock for a book.
    /// </summary>
    public class StockCreateDTO
    {
        public int BookId { get; set; }
        public int Quantity { get; set; }
    }
}