using backend.Models.Enums;

namespace backend.DTOs
{
    /// <summary>
    /// DTO returned when retrieving reservation information, including book and user details.
    /// </summary>
    public class ReservationReadDto
    {
        /// <summary>
        /// Unique identifier of the reservation.
        /// </summary>
        /// <example>23</example>
        public int ReservationId { get; set; }

        /// <summary>
        /// Identifier of the user who made the reservation.
        /// </summary>
        /// <example>7</example>
        public int UserId { get; set; }

        /// <summary>
        /// Full name of the user who made the reservation.
        /// </summary>
        /// <example>Jane Doe</example>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// Identifier of the reserved book.
        /// </summary>
        /// <example>42</example>
        public int BookId { get; set; }

        /// <summary>
        /// Title of the reserved book.
        /// </summary>
        /// <example>The Hobbit</example>
        public string BookTitle { get; set; } = string.Empty;

        /// <summary>
        /// Date and time when the reservation was created.
        /// </summary>
        /// <example>2025-06-20T14:30:00Z</example>
        public DateTime ReservationDate { get; set; }

        /// <summary>
        /// Current status of the reservation.
        /// </summary>
        /// <example>Pending</example>
        public ReservationStatus Status { get; set; }
    }
}