using System.ComponentModel.DataAnnotations;

namespace backend.Models.DTOs
{
    /// <summary>
    /// DTO used to create a new loan.
    /// </summary>
    public class LoanCreateDTO
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int BookId { get; set; }
    }
}