using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    /// <summary>
    /// Records a user-centric event (loan, return, reservation, etc.) for audit and history purposes.
    /// </summary>
    public class History
    {
        /// <summary>
        /// Primary key of the history record.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int HistoryId { get; set; }

        /// <summary>
        /// Identifier of the user associated with this event.
        /// </summary>
        [Required(ErrorMessage = "UserId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "UserId must be a positive integer.")]
        public int UserId { get; set; }

        /// <summary>
        /// Optional identifier of the related loan.
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "LoanId must be a positive integer.")]
        public int? LoanId { get; set; }

        /// <summary>
        /// Optional identifier of the related reservation.
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "ReservationId must be a positive integer.")]
        public int? ReservationId { get; set; }

        /// <summary>
        /// Exact date and time when the event occurred.
        /// </summary>
        [Required(ErrorMessage = "EventDate is required.")]
        public DateTime EventDate { get; set; }

        /// <summary>
        /// Type of event: "Loan", "Return", "Reservation", "Cancel", etc.
        /// </summary>
        [Required(ErrorMessage = "EventType is required.")]
        [StringLength(50, ErrorMessage = "EventType cannot exceed 50 characters.")]
        public string EventType { get; set; } = string.Empty;

        /// <summary>
        /// Navigation property to the user who triggered the event.
        /// </summary>
        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;

        /// <summary>
        /// Navigation property to the related loan, if applicable.
        /// </summary>
        [ForeignKey(nameof(LoanId))]
        public Loan? Loan { get; set; }

        /// <summary>
        /// Navigation property to the related reservation, if applicable.
        /// </summary>
        [ForeignKey(nameof(ReservationId))]
        public Reservation? Reservation { get; set; }
    }
}
