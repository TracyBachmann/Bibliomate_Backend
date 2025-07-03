using System;

namespace BackendBiblioMate.DTOs
{
    /// <summary>
    /// DTO returned when retrieving loan information, including user and book details.
    /// Contains all fields relevant to the loan lifecycle and its status.
    /// </summary>
    public class LoanReadDto
    {
        /// <summary>
        /// Gets the unique identifier of the loan.
        /// </summary>
        /// <example>15</example>
        public int LoanId { get; init; }

        /// <summary>
        /// Gets the identifier of the user who borrowed the book.
        /// </summary>
        /// <example>7</example>
        public int UserId { get; init; }

        /// <summary>
        /// Gets the full name of the user who borrowed the book.
        /// </summary>
        /// <example>Jane Doe</example>
        public string UserName { get; init; } = string.Empty;

        /// <summary>
        /// Gets the identifier of the borrowed book.
        /// </summary>
        /// <example>42</example>
        public int BookId { get; init; }

        /// <summary>
        /// Gets the title of the borrowed book.
        /// </summary>
        /// <example>The Hobbit</example>
        public string BookTitle { get; init; } = string.Empty;

        /// <summary>
        /// Gets the date when the loan started (UTC).
        /// </summary>
        /// <example>2025-06-01T10:30:00Z</example>
        public DateTime LoanDate { get; init; }

        /// <summary>
        /// Gets the date when the book is due to be returned (UTC).
        /// </summary>
        /// <example>2025-06-15T10:30:00Z</example>
        public DateTime DueDate { get; init; }

        /// <summary>
        /// Gets the date when the book was actually returned, if applicable (UTC).
        /// </summary>
        /// <example>2025-06-14T16:45:00Z</example>
        public DateTime? ReturnDate { get; init; }

        /// <summary>
        /// Gets the fine amount charged for late return, if any.
        /// </summary>
        /// <example>0.00</example>
        public decimal Fine { get; init; }
    }
}
