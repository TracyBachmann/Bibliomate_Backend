using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class RecommendationService
    {
        [Key]
        public int RecommendationId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int RecommendationBookId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        [ForeignKey("RecommendationBookId")]
        public Book RecommendationBook { get; set; } = null!;
    }
}