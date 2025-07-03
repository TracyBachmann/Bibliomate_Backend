using System.ComponentModel.DataAnnotations;

namespace BackendBiblioMate.DTOs
{
    /// <summary>
    /// DTO used to update the due date of an existing loan.
    /// Contains the new due date to apply to the loan record.
    /// </summary>
    public class LoanUpdateDto
    {
        /// <summary>
        /// Gets the new due date for the loan (UTC).
        /// </summary>
        /// <example>2025-07-15T10:30:00Z</example>
        [Required(ErrorMessage = "DueDate is required.")]
        [DataType(DataType.DateTime, ErrorMessage = "DueDate must be a valid date and time.")]
        public DateTime DueDate { get; init; }
    }
}