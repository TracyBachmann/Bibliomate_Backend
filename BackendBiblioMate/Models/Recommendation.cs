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
        /// Gets or sets the unique identifier of the recommendation.
        /// </summary>
        /// <example>1</example>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RecommendationId { get; init; }

        /// <summary>
        /// Gets or sets the identifier of the user for whom the recommendation is made.
        /// </summary>
        /// <example>7</example>
        [Required(ErrorMessage = "UserId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "UserId must be a positive integer.")]
        public int UserId { get; init; }

        /// <summary>
        /// Gets or sets the identifier of the recommended book.
        /// </summary>
        /// <example>42</example>
        [Required(ErrorMessage = "BookId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "BookId must be a positive integer.")]
        public int BookId { get; init; }

        /// <summary>
        /// Gets or sets the navigation property for the user receiving the recommendation.
        /// </summary>
        [ForeignKey(nameof(UserId))]
        public User User { get; init; } = null!;

        /// <summary>
        /// Gets or sets the navigation property for the recommended book.
        /// </summary>
        [ForeignKey(nameof(BookId))]
        public Book Book { get; init; } = null!;
    }
}