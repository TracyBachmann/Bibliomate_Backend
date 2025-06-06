using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class AuthentificationService
    {
        [Key]
        public int AuthId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public DateTime LastLogin { get; set; }

        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }
    }
}