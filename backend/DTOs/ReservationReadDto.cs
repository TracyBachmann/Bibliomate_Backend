// ReservationReadDto.cs
namespace backend.DTOs
{
    /// <summary>
    /// DTO used to read reservation information, including book and user.
    /// </summary>
    public class ReservationReadDto
    {
        public int ReservationId { get; set; }

        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;

        public int BookId { get; set; }
        public string BookTitle { get; set; } = string.Empty;

        public DateTime ReservationDate { get; set; }
    }
}