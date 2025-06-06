using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;

namespace backend.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Password { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Address { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Phone { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Role { get; set; } = "User"; // ou UserRole enum si adopté

        public ICollection<Loan>? Loans { get; set; }
        public ICollection<Reservation>? Reservations { get; set; }
        public HistoryService? HistoryService { get; set; }
        public AccountService? AccountService { get; set; }
        public AuthenticationService? AuthenticationService { get; set; }
        public ICollection<Report>? Reports { get; set; }
        public ICollection<RecommendationService>? Recommendations { get; set; }
        public ICollection<Notification>? Notifications { get; set; }
    }
}
