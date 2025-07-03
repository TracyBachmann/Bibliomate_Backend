using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendBiblioMate.Models
{
    /// <summary>
    /// Represents a specific level within a shelf, containing books.
    /// Defines capacity and current load constraints for each level.
    /// </summary>
    public class ShelfLevel
    {
        /// <summary>
        /// Gets the primary key of the shelf level.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ShelfLevelId { get; init; }

        /// <summary>
        /// Gets or sets the identifier of the parent shelf.
        /// </summary>
        [Required(ErrorMessage = "ShelfId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "ShelfId must be a positive integer.")]
        public int ShelfId { get; set; }

        /// <summary>
        /// Gets or sets the numeric level on the shelf (e.g., 1 for the bottom level).
        /// </summary>
        [Required(ErrorMessage = "LevelNumber is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "LevelNumber must be a positive integer.")]
        public int LevelNumber { get; set; }

        /// <summary>
        /// Gets or sets the maximum height capacity in centimeters (optional).
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "MaxHeight must be zero or a positive integer.")]
        public int MaxHeight { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of books this level can hold (optional).
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "Capacity must be zero or a positive integer.")]
        public int Capacity { get; set; }

        /// <summary>
        /// Gets or sets the current number of books on this level (optional).
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "CurrentLoad must be zero or a positive integer.")]
        public int CurrentLoad { get; set; }

        /// <summary>
        /// Navigation property for the parent shelf.
        /// </summary>
        [ForeignKey(nameof(ShelfId))]
        public Shelf Shelf { get; init; } = null!;

        /// <summary>
        /// Gets the collection of books placed on this shelf level.
        /// </summary>
        public ICollection<Book> Books { get; init; } = new List<Book>();
    }
}