using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    /// <summary>
    /// Represents a specific level within a shelf, containing books.
    /// </summary>
    public class ShelfLevel
    {
        /// <summary>
        /// Primary key of the shelf level.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ShelfLevelId { get; set; }

        /// <summary>
        /// Identifier of the parent shelf.
        /// </summary>
        [Required(ErrorMessage = "ShelfId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "ShelfId must be a positive integer.")]
        public int ShelfId { get; set; }

        /// <summary>
        /// Numeric level on the shelf (e.g., 1 for the bottom level).
        /// </summary>
        [Required(ErrorMessage = "LevelNumber is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "LevelNumber must be a positive integer.")]
        public int LevelNumber { get; set; }

        /// <summary>
        /// Maximum height capacity in centimeters (optional).
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "MaxHeight must be zero or a positive integer.")]
        public int MaxHeight { get; set; }

        /// <summary>
        /// Maximum number of books this level can hold (optional).
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "Capacity must be zero or a positive integer.")]
        public int Capacity { get; set; }

        /// <summary>
        /// Current number of books on this level (optional).
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "CurrentLoad must be zero or a positive integer.")]
        public int CurrentLoad { get; set; }

        /// <summary>
        /// Navigation property for the parent shelf.
        /// </summary>
        [ForeignKey(nameof(ShelfId))]
        public Shelf Shelf { get; set; } = null!;

        /// <summary>
        /// Collection of books placed on this shelf level.
        /// </summary>
        public ICollection<Book> Books { get; set; } = new List<Book>();
    }
}
