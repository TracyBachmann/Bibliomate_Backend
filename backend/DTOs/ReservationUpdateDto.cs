using System.ComponentModel.DataAnnotations;
using backend.Models.Enums;

namespace backend.DTOs
{
    /// <summary>
    /// DTO used to update an existing reservation.
    /// </summary>
    public class ReservationUpdateDto
    {
        [Required]
        public int ReservationId { get; set; }

        [Required]
        public int BookId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public DateTime ReservationDate { get; set; }

        [Required]
        public ReservationStatus Status { get; set; }
    }
}