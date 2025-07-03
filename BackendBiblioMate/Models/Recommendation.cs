using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendBiblioMate.Models
{
    /// <summary>
    /// Represents a book recommendation generated for a specific user.
    /// Links a user to a recommended book in the system.
    /// </summary>
    public class Recommendation
    {
        /// <summary>
        /// Gets the primary key of the recommendation record.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RecommendationId { get; init; }

        /// <summary>
        /// Gets or sets the identifier of the user for whom the recommendation is made.
        /// </summary>
        [Required(ErrorMessage = "UserId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "UserId must be a positive integer.")]
        public int UserId { get; init; }

        /// <summary>
        /// Gets or sets the identifier of the recommended book.
        /// </summary>
        [Required(ErrorMessage = "RecommendationBookId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "RecommendationBookId must be a positive integer.")]
        public int RecommendationBookId { get; init; }

        /// <summary>
        /// Navigation property for the user receiving the recommendation.
        /// </summary>
        [ForeignKey(nameof(UserId))]
        public User User { get; init; } = null!;

        /// <summary>
        /// Navigation property for the book being recommended.
        /// </summary>
        [ForeignKey(nameof(RecommendationBookId))]
        public Book RecommendationBook { get; init; } = null!;
    }
}