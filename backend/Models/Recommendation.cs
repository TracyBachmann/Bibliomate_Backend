using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    /// <summary>
    /// Represents a book recommendation generated for a specific user.
    /// </summary>
    public class Recommendation
    {
        /// <summary>
        /// Primary key of the recommendation record.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RecommendationId { get; set; }

        /// <summary>
        /// Identifier of the user for whom the recommendation is made.
        /// </summary>
        [Required(ErrorMessage = "UserId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "UserId must be a positive integer.")]
        public int UserId { get; set; }

        /// <summary>
        /// Identifier of the recommended book.
        /// </summary>
        [Required(ErrorMessage = "RecommendationBookId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "RecommendationBookId must be a positive integer.")]
        public int RecommendationBookId { get; set; }

        /// <summary>
        /// Navigation property for the user.
        /// </summary>
        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;

        /// <summary>
        /// Navigation property for the recommended book.
        /// </summary>
        [ForeignKey(nameof(RecommendationBookId))]
        public Book RecommendationBook { get; set; } = null!;
    }
}