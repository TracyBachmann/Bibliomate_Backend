using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BackendBiblioMate.Models.Enums;

namespace BackendBiblioMate.Models
{
    /// <summary>
    /// Represents a user's reservation for a book, including status and timestamps.
    /// </summary>
    public class Reservation
    {
        /// <summary>
        /// Gets or sets the unique identifier of the reservation.
        /// </summary>
        /// <example>23</example>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ReservationId { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the reserved book.
        /// </summary>
        /// <example>42</example>
        [Required(ErrorMessage = "BookId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "BookId must be a positive integer.")]
        public int BookId { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the user who made the reservation.
        /// </summary>
        /// <example>7</example>
        [Required(ErrorMessage = "UserId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "UserId must be a positive integer.")]
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the UTC timestamp when the reservation record was created.
        /// </summary>
        /// <example>2025-06-20T12:45:00Z</example>
        [Required(ErrorMessage = "CreatedAt is required.")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the effective date of the reservation (when the reservation becomes active).
        /// </summary>
        /// <example>2025-06-21T09:00:00Z</example>
        [Required(ErrorMessage = "ReservationDate is required.")]
        public DateTime ReservationDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the current status of the reservation.
        /// </summary>
        /// <remarks>
        /// Possible values: Pending, Completed, Cancelled, Expired.
        /// </remarks>
        /// <example>Pending</example>
        [Required(ErrorMessage = "Status is required.")]
        [EnumDataType(typeof(ReservationStatus), ErrorMessage = "Invalid reservation status.")]
        public ReservationStatus Status { get; set; } = ReservationStatus.Pending;

        /// <summary>
        /// Gets or sets the identifier of the stock entry assigned to this reservation (if available).
        /// </summary>
        /// <example>15</example>
        [Range(1, int.MaxValue, ErrorMessage = "AssignedStockId must be a positive integer.")]
        public int? AssignedStockId { get; set; }

        /// <summary>
        /// Gets or sets the UTC timestamp when the reserved book became available for pickup.
        /// </summary>
        /// <example>2025-06-22T08:30:00Z</example>
        public DateTime? AvailableAt { get; set; }

        /// <summary>
        /// Navigation property for the reserved book.
        /// </summary>
        [ForeignKey(nameof(BookId))]
        public Book Book { get; set; } = null!;

        /// <summary>
        /// Navigation property for the user who made the reservation.
        /// </summary>
        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;
    }
}

