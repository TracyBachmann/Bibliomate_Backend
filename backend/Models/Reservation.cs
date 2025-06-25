using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using backend.Models.Enums;

namespace backend.Models
{
    /// <summary>
    /// Represents a user’s reservation for a book.
    /// </summary>
    public class Reservation
    {
        /// <summary>
        /// Primary key of the reservation.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ReservationId { get; set; }

        /// <summary>
        /// Identifier of the reserved book.
        /// </summary>
        [Required(ErrorMessage = "BookId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "BookId must be a positive integer.")]
        public int BookId { get; set; }

        /// <summary>
        /// Identifier of the user who made the reservation.
        /// </summary>
        [Required(ErrorMessage = "UserId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "UserId must be a positive integer.")]
        public int UserId { get; set; }

        /// <summary>
        /// Date and time when the reservation becomes effective.
        /// </summary>
        [Required(ErrorMessage = "ReservationDate is required.")]
        public DateTime ReservationDate { get; set; }

        /// <summary>
        /// Current status of the reservation.
        /// </summary>
        [Required(ErrorMessage = "Status is required.")]
        [EnumDataType(typeof(ReservationStatus), ErrorMessage = "Invalid reservation status.")]
        public ReservationStatus Status { get; set; } = ReservationStatus.Pending;

        /// <summary>
        /// Identifier of the assigned stock entry, when available.
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "AssignedStockId must be a positive integer.")]
        public int? AssignedStockId { get; set; }

        /// <summary>
        /// Date and time when the reserved book became available for pickup.
        /// </summary>
        public DateTime? AvailableAt { get; set; }

        /// <summary>
        /// Date and time when the reservation was created.
        /// </summary>
        [Required(ErrorMessage = "CreatedAt is required.")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

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
