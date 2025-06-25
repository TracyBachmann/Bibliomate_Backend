using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    /// <summary>
    /// Junction entity linking users and their preferred genres for personalized recommendations.
    /// </summary>
    public class UserGenre
    {
        /// <summary>
        /// Foreign key referencing the user.
        /// </summary>
        [Required(ErrorMessage = "UserId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "UserId must be a positive integer.")]
        public int UserId { get; set; }

        /// <summary>
        /// Navigation property for the user.
        /// </summary>
        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;

        /// <summary>
        /// Foreign key referencing the preferred genre.
        /// </summary>
        [Required(ErrorMessage = "GenreId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "GenreId must be a positive integer.")]
        public int GenreId { get; set; }

        /// <summary>
        /// Navigation property for the genre.
        /// </summary>
        [ForeignKey(nameof(GenreId))]
        public Genre Genre { get; set; } = null!;
    }
}