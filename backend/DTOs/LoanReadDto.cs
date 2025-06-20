namespace backend.DTOs
{
    /// <summary>
    /// DTO used to read loan information, including user and book details.
    /// </summary>
    public class LoanReadDto
    {
        public int LoanId { get; set; }

        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;

        public int BookId { get; set; }
        public string BookTitle { get; set; } = string.Empty;

        public DateTime LoanDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }

        public decimal Fine { get; set; }
    }
}