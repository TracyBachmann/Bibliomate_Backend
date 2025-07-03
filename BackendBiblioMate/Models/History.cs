using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendBiblioMate.Models
{
    /// <summary>
    /// Records a user-centric event (loan, return, reservation, etc.) for audit and history purposes.
    /// </summary>
    public class History
    {
        /// <summary>
        /// Gets or sets the primary key of the history record.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int HistoryId { get; init; }

        /// <summary>
        /// Gets or sets the identifier of the user associated with this event.
        /// </summary>
        [Required(ErrorMessage = "UserId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "UserId must be a positive integer.")]
        public int UserId { get; init; }

        /// <summary>
        /// Gets or sets the optional identifier of the related loan.
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "LoanId must be a positive integer.")]
        public int? LoanId { get; init; }

        /// <summary>
        /// Gets or sets the optional identifier of the related reservation.
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "ReservationId must be a positive integer.")]
        public int? ReservationId { get; init; }

        /// <summary>
        /// Gets or sets the exact date and time when the event occurred.
        /// </summary>
        [Required(ErrorMessage = "EventDate is required.")]
        public DateTime EventDate { get; init; }

        /// <summary>
        /// Gets or sets the type of event: "Loan", "Return", "Reservation", "Cancel", etc.
        /// </summary>
        [Required(ErrorMessage = "EventType is required.")]
        [StringLength(50, ErrorMessage = "EventType cannot exceed 50 characters.")]
        public string EventType { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the navigation property to the user who triggered the event.
        /// </summary>
        [ForeignKey(nameof(UserId))]
        public User User { get; init; } = null!;

        /// <summary>
        /// Gets or sets the navigation property to the related loan, if applicable.
        /// </summary>
        [ForeignKey(nameof(LoanId))]
        public Loan? Loan { get; init; }

        /// <summary>
        /// Gets or sets the navigation property to the related reservation, if applicable.
        /// </summary>
        [ForeignKey(nameof(ReservationId))]
        public Reservation? Reservation { get; init; }
    }
}