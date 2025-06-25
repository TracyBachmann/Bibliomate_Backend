namespace backend.DTOs
{
    /// <summary>
    /// DTO returned when retrieving loan information, including user and book details.
    /// </summary>
    public class LoanReadDto
    {
        /// <summary>
        /// Unique identifier of the loan.
        /// </summary>
        /// <example>15</example>
        public int LoanId { get; set; }

        /// <summary>
        /// Identifier of the user who borrowed the book.
        /// </summary>
        /// <example>7</example>
        public int UserId { get; set; }

        /// <summary>
        /// Full name of the user who borrowed the book.
        /// </summary>
        /// <example>Jane Doe</example>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// Identifier of the borrowed book.
        /// </summary>
        /// <example>42</example>
        public int BookId { get; set; }

        /// <summary>
        /// Title of the borrowed book.
        /// </summary>
        /// <example>The Hobbit</example>
        public string BookTitle { get; set; } = string.Empty;

        /// <summary>
        /// Date when the loan started.
        /// </summary>
        /// <example>2025-06-01T10:30:00Z</example>
        public DateTime LoanDate { get; set; }

        /// <summary>
        /// Date when the book is due to be returned.
        /// </summary>
        /// <example>2025-06-15T10:30:00Z</example>
        public DateTime DueDate { get; set; }

        /// <summary>
        /// Date when the book was actually returned, if applicable.
        /// </summary>
        /// <example>2025-06-14T16:45:00Z</example>
        public DateTime? ReturnDate { get; set; }

        /// <summary>
        /// Fine amount charged for late return, if any.
        /// </summary>
        /// <example>0.00</example>
        public decimal Fine { get; set; }
    }
}