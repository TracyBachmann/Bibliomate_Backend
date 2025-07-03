using BackendBiblioMate.Models.Enums;

namespace BackendBiblioMate.DTOs
{
    /// <summary>
    /// DTO returned when retrieving reservation information, including book and user details.
    /// Contains all relevant fields to display reservation status and metadata.
    /// </summary>
    public class ReservationReadDto
    {
        /// <summary>
        /// Gets the unique identifier of the reservation.
        /// </summary>
        /// <example>23</example>
        public int ReservationId { get; init; }

        /// <summary>
        /// Gets the identifier of the user who made the reservation.
        /// </summary>
        /// <example>7</example>
        public int UserId { get; init; }

        /// <summary>
        /// Gets the full name of the user who made the reservation.
        /// </summary>
        /// <example>Jane Doe</example>
        public string UserName { get; init; } = string.Empty;

        /// <summary>
        /// Gets the identifier of the reserved book.
        /// </summary>
        /// <example>42</example>
        public int BookId { get; init; }

        /// <summary>
        /// Gets the title of the reserved book.
        /// </summary>
        /// <example>The Hobbit</example>
        public string BookTitle { get; init; } = string.Empty;

        /// <summary>
        /// Gets the date and time when the reservation was created (UTC).
        /// </summary>
        /// <example>2025-06-20T14:30:00Z</example>
        public DateTime ReservationDate { get; init; }

        /// <summary>
        /// Gets the current status of the reservation.
        /// </summary>
        /// <remarks>
        /// Possible values include Pending, Completed, Cancelled.
        /// </remarks>
        /// <example>Pending</example>
        public ReservationStatus Status { get; init; }
    }
}