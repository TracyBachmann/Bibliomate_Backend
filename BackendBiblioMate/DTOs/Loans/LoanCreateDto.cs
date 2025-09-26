using System.ComponentModel.DataAnnotations;

namespace BackendBiblioMate.DTOs
{
    /// <summary>
    /// Data Transfer Object used to create a new loan (book borrowing).
    /// </summary>
    public class LoanCreateDto
    {
        /// <summary>
        /// Gets or sets the identifier of the borrowing user.
        /// </summary>
        /// <remarks>
        /// - Optional: if omitted, it is inferred from the authenticated token (standard user).  
        /// - Required only when staff members create loans for other users.  
        /// </remarks>
        /// <example>7</example>
        public int? UserId { get; init; }

        /// <summary>
        /// Gets or sets the identifier of the book to be borrowed.
        /// </summary>
        /// <example>42</example>
        [Required(ErrorMessage = "BookId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "BookId must be a positive integer.")]
        public int BookId { get; init; }
    }
}