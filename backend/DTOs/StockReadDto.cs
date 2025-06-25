namespace backend.DTOs
{
    /// <summary>
    /// DTO returned when retrieving stock details, including the related book title.
    /// </summary>
    public class StockReadDto
    {
        /// <summary>
        /// Unique identifier of the stock entry.
        /// </summary>
        /// <example>15</example>
        public int StockId { get; set; }

        /// <summary>
        /// Identifier of the book associated with this stock entry.
        /// </summary>
        /// <example>42</example>
        public int BookId { get; set; }

        /// <summary>
        /// Title of the book associated with this stock entry.
        /// </summary>
        /// <example>The Hobbit</example>
        public string BookTitle { get; set; } = string.Empty;

        /// <summary>
        /// Current quantity available in stock.
        /// </summary>
        /// <example>7</example>
        public int Quantity { get; set; }

        /// <summary>
        /// Indicates whether at least one copy is currently available.
        /// </summary>
        /// <example>true</example>
        public bool IsAvailable { get; set; }
    }
}