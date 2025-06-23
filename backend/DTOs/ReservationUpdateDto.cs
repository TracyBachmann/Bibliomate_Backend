using System;
using System.ComponentModel.DataAnnotations;
using backend.Models.Enums;

namespace backend.DTOs
{
    /// <summary>
    /// DTO used to update an existing reservation.
    /// </summary>
    public class ReservationUpdateDto
    {
        /// <summary>
        /// Unique identifier of the reservation to update.
        /// </summary>
        /// <example>23</example>
        [Required(ErrorMessage = "ReservationId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "ReservationId must be a positive integer.")]
        public int ReservationId { get; set; }

        /// <summary>
        /// Identifier of the book associated with the reservation.
        /// </summary>
        /// <example>42</example>
        [Required(ErrorMessage = "BookId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "BookId must be a positive integer.")]
        public int BookId { get; set; }

        /// <summary>
        /// Identifier of the user who made the reservation.
        /// </summary>
        /// <example>7</example>
        [Required(ErrorMessage = "UserId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "UserId must be a positive integer.")]
        public int UserId { get; set; }

        /// <summary>
        /// Date and time when the reservation was created.
        /// </summary>
        /// <example>2025-06-20T14:30:00Z</example>
        [Required(ErrorMessage = "ReservationDate is required.")]
        [DataType(DataType.DateTime, ErrorMessage = "ReservationDate must be a valid date and time.")]
        public DateTime ReservationDate { get; set; }

        /// <summary>
        /// Updated status of the reservation.
        /// </summary>
        /// <example>Available</example>
        [Required(ErrorMessage = "Status is required.")]
        [EnumDataType(typeof(ReservationStatus), ErrorMessage = "Invalid reservation status.")]
        public ReservationStatus Status { get; set; }
    }
}