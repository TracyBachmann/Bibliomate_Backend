namespace BackendBiblioMate.DTOs
{
    /// <summary>
    /// DTO returned when retrieving stock details, including the related book title.
    /// Contains the current quantity and availability status.
    /// </summary>
    public class StockReadDto
    {
        /// <summary>
        /// Gets the unique identifier of the stock entry.
        /// </summary>
        /// <example>15</example>
        public int StockId { get; init; }

        /// <summary>
        /// Gets the identifier of the book associated with this stock entry.
        /// </summary>
        /// <example>42</example>
        public int BookId { get; init; }

        /// <summary>
        /// Gets the title of the book associated with this stock entry.
        /// </summary>
        /// <example>The Hobbit</example>
        public string BookTitle { get; init; } = string.Empty;

        /// <summary>
        /// Gets the current quantity available in stock.
        /// </summary>
        /// <example>7</example>
        public int Quantity { get; init; }

        /// <summary>
        /// Gets a value indicating whether at least one copy is currently available.
        /// </summary>
        /// <example>true</example>
        public bool IsAvailable { get; init; }
    }
}