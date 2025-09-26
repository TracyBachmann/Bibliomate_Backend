using System.ComponentModel.DataAnnotations;

namespace BackendBiblioMate.DTOs
{
    /// <summary>
    /// Data Transfer Object used to update the due date of an existing loan.
    /// </summary>
    public class LoanUpdateDto
    {
        /// <summary>
        /// Gets or sets the new due date for the loan, in UTC.
        /// </summary>
        /// <example>2025-07-15T10:30:00Z</example>
        [Required(ErrorMessage = "DueDate is required.")]
        [DataType(DataType.DateTime, ErrorMessage = "DueDate must be a valid date and time.")]
        public DateTime DueDate { get; init; }
    }
}