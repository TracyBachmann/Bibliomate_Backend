using System.ComponentModel.DataAnnotations;

namespace backend.DTOs
{
    /// <summary>
    /// DTO used to update an existing loan.
    /// </summary>
    public class LoanUpdateDto
    {
        /// <summary>
        /// Unique identifier of the loan to update.
        /// </summary>
        /// <example>15</example>
        [Required(ErrorMessage = "LoanId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "LoanId must be a positive integer.")]
        public int LoanId { get; set; }

        /// <summary>
        /// Identifier of the user associated with the loan.
        /// </summary>
        /// <example>7</example>
        [Required(ErrorMessage = "UserId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "UserId must be a positive integer.")]
        public int UserId { get; set; }

        /// <summary>
        /// Identifier of the book associated with the loan.
        /// </summary>
        /// <example>42</example>
        [Required(ErrorMessage = "BookId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "BookId must be a positive integer.")]
        public int BookId { get; set; }

        /// <summary>
        /// Date when the loan started.
        /// </summary>
        /// <example>2025-06-01T10:30:00Z</example>
        [Required(ErrorMessage = "LoanDate is required.")]
        [DataType(DataType.DateTime, ErrorMessage = "LoanDate must be a valid date and time.")]
        public DateTime LoanDate { get; set; }

        /// <summary>
        /// Date when the book is due to be returned.
        /// </summary>
        /// <example>2025-06-15T10:30:00Z</example>
        [Required(ErrorMessage = "DueDate is required.")]
        [DataType(DataType.DateTime, ErrorMessage = "DueDate must be a valid date and time.")]
        public DateTime DueDate { get; set; }

        /// <summary>
        /// Actual return date of the book, if it has been returned.
        /// </summary>
        /// <example>2025-06-14T16:45:00Z</example>
        [DataType(DataType.DateTime, ErrorMessage = "ReturnDate must be a valid date and time.")]
        public DateTime? ReturnDate { get; set; }

        /// <summary>
        /// Fine amount charged for a late return, if applicable.
        /// </summary>
        /// <example>5.00</example>
        [Range(0, double.MaxValue, ErrorMessage = "Fine must be zero or a positive value.")]
        public decimal Fine { get; set; }
    }
}
