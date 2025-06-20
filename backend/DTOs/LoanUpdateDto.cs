using System.ComponentModel.DataAnnotations;

namespace backend.DTOs
{
    /// <summary>
    /// DTO used to update an existing loan.
    /// </summary>
    public class LoanUpdateDto
    {
        [Required]
        public int LoanId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int BookId { get; set; }

        [Required]
        public DateTime LoanDate { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        public DateTime? ReturnDate { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Fine { get; set; }
    }
}