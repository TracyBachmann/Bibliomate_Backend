using BackendBiblioMate.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace BackendBiblioMate.DTOs
{
    /// <summary>
    /// Data Transfer Object used to update an existing reservation.
    /// Contains all fields that can be modified on a reservation record.
    /// </summary>
    public class ReservationUpdateDto
    {
        /// <summary>
        /// Gets or sets the unique identifier of the reservation to update.
        /// </summary>
        /// <example>23</example>
        [Required(ErrorMessage = "ReservationId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "ReservationId must be a positive integer.")]
        public int ReservationId { get; init; }

        /// <summary>
        /// Gets or sets the identifier of the book associated with the reservation.
        /// </summary>
        /// <example>42</example>
        [Required(ErrorMessage = "BookId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "BookId must be a positive integer.")]
        public int BookId { get; init; }

        /// <summary>
        /// Gets or sets the identifier of the user who made the reservation.
        /// </summary>
        /// <example>7</example>
        [Required(ErrorMessage = "UserId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "UserId must be a positive integer.")]
        public int UserId { get; init; }

        /// <summary>
        /// Gets or sets the date and time when the reservation was created, in UTC.
        /// </summary>
        /// <example>2025-06-20T14:30:00Z</example>
        [Required(ErrorMessage = "ReservationDate is required.")]
        [DataType(DataType.DateTime, ErrorMessage = "ReservationDate must be a valid date and time.")]
        public DateTime ReservationDate { get; init; }

        /// <summary>
        /// Gets or sets the updated status of the reservation.
        /// </summary>
        /// <remarks>
        /// Must be one of the defined <see cref="ReservationStatus"/> values.
        /// </remarks>
        /// <example>Cancelled</example>
        [Required(ErrorMessage = "Status is required.")]
        [EnumDataType(typeof(ReservationStatus), ErrorMessage = "Invalid reservation status.")]
        public ReservationStatus Status { get; init; }
    }
}