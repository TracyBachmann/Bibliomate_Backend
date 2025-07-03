using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendBiblioMate.Models
{
    /// <summary>
    /// Junction entity linking users and their preferred genres for personalized recommendations.
    /// </summary>
    public class UserGenre
    {
        /// <summary>
        /// Gets or sets the foreign key referencing the user.
        /// </summary>
        [Required(ErrorMessage = "UserId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "UserId must be a positive integer.")]
        public int UserId { get; init; }

        /// <summary>
        /// Navigation property for the user.
        /// </summary>
        [ForeignKey(nameof(UserId))]
        public User User { get; init; } = null!;

        /// <summary>
        /// Gets or sets the foreign key referencing the preferred genre.
        /// </summary>
        [Required(ErrorMessage = "GenreId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "GenreId must be a positive integer.")]
        public int GenreId { get; init; }

        /// <summary>
        /// Navigation property for the genre.
        /// </summary>
        [ForeignKey(nameof(GenreId))]
        public Genre Genre { get; init; } = null!;
    }
}