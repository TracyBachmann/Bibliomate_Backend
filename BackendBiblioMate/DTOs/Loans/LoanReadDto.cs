namespace BackendBiblioMate.DTOs
{
    /// <summary>
    /// Data Transfer Object returned when retrieving loan information.
    /// Contains user, book, and lifecycle details for a loan.
    /// </summary>
    public class LoanReadDto
    {
        /// <summary>
        /// Gets or sets the unique identifier of the loan.
        /// </summary>
        /// <example>15</example>
        public int LoanId { get; init; }

        /// <summary>
        /// Gets or sets the identifier of the user who borrowed the book.
        /// </summary>
        /// <example>7</example>
        public int UserId { get; init; }

        /// <summary>
        /// Gets or sets the full name of the user who borrowed the book.
        /// </summary>
        /// <example>Jane Doe</example>
        public string UserName { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the identifier of the borrowed book.
        /// </summary>
        /// <example>42</example>
        public int BookId { get; init; }

        /// <summary>
        /// Gets or sets the title of the borrowed book.
        /// </summary>
        /// <example>The Hobbit</example>
        public string BookTitle { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the date when the loan started, in UTC.
        /// </summary>
        /// <example>2025-06-01T10:30:00Z</example>
        public DateTime LoanDate { get; init; }

        /// <summary>
        /// Gets or sets the due date for returning the book, in UTC.
        /// </summary>
        /// <example>2025-06-15T10:30:00Z</example>
        public DateTime DueDate { get; init; }

        /// <summary>
        /// Gets or sets the date when the book was actually returned, if applicable, in UTC.
        /// </summary>
        /// <example>2025-06-14T16:45:00Z</example>
        public DateTime? ReturnDate { get; init; }

        /// <summary>
        /// Gets or sets the fine amount charged for late return, if any.
        /// </summary>
        /// <example>0.00</example>
        public decimal Fine { get; init; }
    }
}

