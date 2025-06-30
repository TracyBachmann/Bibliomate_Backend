using System.ComponentModel.DataAnnotations;

namespace backend.DTOs
{
    /// <summary>
    /// DTO used to update the due date of an existing loan.
    /// </summary>
    public class LoanUpdateDto
    {
        /// <summary>
        /// New due date for the loan.
        /// </summary>
        [Required(ErrorMessage = "DueDate is required.")]
        [DataType(DataType.DateTime, ErrorMessage = "DueDate must be a valid date and time.")]
        public DateTime DueDate { get; set; }
    }
}