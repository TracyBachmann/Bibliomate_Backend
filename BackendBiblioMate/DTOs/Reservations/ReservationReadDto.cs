using BackendBiblioMate.Models.Enums;

namespace BackendBiblioMate.DTOs
{
    /// <summary>
    /// Data Transfer Object returned when retrieving reservation information,
    /// including book and user details. Contains all relevant fields to display
    /// reservation status and metadata.
    /// </summary>
    public class ReservationReadDto
    {
        /// <summary>
        /// Gets or sets the unique identifier of the reservation.
        /// </summary>
        /// <example>23</example>
        public int ReservationId { get; init; }

        /// <summary>
        /// Gets or sets the identifier of the user who made the reservation.
        /// </summary>
        /// <example>7</example>
        public int UserId { get; init; }

        /// <summary>
        /// Gets or sets the full name of the user who made the reservation.
        /// </summary>
        /// <example>Jane Doe</example>
        public string UserName { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the identifier of the reserved book.
        /// </summary>
        /// <example>42</example>
        public int BookId { get; init; }

        /// <summary>
        /// Gets or sets the title of the reserved book.
        /// </summary>
        /// <example>The Hobbit</example>
        public string BookTitle { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the date and time when the reservation was created, in UTC.
        /// </summary>
        /// <example>2025-06-20T14:30:00Z</example>
        public DateTime ReservationDate { get; init; }

        /// <summary>
        /// Gets or sets the current status of the reservation.
        /// </summary>
        /// <remarks>
        /// Possible values include:
        /// - <c>Pending</c> (waiting for availability)  
        /// - <c>Completed</c> (fulfilled by a loan)  
        /// - <c>Cancelled</c> (manually cancelled or expired)  
        /// </remarks>
        /// <example>Pending</example>
        public ReservationStatus Status { get; init; }

        /// <summary>
        /// Gets or sets the expiration date of the reservation (UTC), if applicable.
        /// Returns <c>null</c> if the reservation has no expiration date.
        /// </summary>
        /// <example>2025-07-01T23:59:59Z</example>
        public DateTime? ExpirationDate { get; init; }
    }
}
