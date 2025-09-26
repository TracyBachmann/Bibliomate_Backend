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
        /// Gets the unique identifier of the shelf level.
        /// </summary>
        /// <example>5</example>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ShelfLevelId { get; init; }

        /// <summary>
        /// Gets or sets the identifier of the parent shelf.
        /// </summary>
        /// <example>3</example>
        [Range(1, int.MaxValue, ErrorMessage = "ShelfId must be a positive integer.")]
        public int ShelfId { get; set; }

        /// <summary>
        /// Gets or sets the numeric level on the shelf.
        /// </summary>
        /// <remarks>
        /// 1 corresponds to the bottom level, increasing upwards.
        /// </remarks>
        /// <example>1</example>
        [Range(1, int.MaxValue, ErrorMessage = "LevelNumber must be a positive integer.")]
        public int LevelNumber { get; set; }

        /// <summary>
        /// Gets or sets the maximum book height in centimeters that this level can accommodate.
        /// </summary>
        /// <remarks>
        /// Zero indicates no height limit.
        /// </remarks>
        /// <example>30</example>
        [Range(0, int.MaxValue, ErrorMessage = "MaxHeight must be zero or a positive integer.")]
        public int MaxHeight { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of books this level can hold.
        /// </summary>
        /// <remarks>
        /// Zero indicates unlimited capacity.
        /// </remarks>
        /// <example>20</example>
        [Range(0, int.MaxValue, ErrorMessage = "Capacity must be zero or a positive integer.")]
        public int Capacity { get; set; }

        /// <summary>
        /// Gets or sets the current number of books stored on this level.
        /// </summary>
        /// <example>12</example>
        [Range(0, int.MaxValue, ErrorMessage = "CurrentLoad must be zero or a positive integer.")]
        public int CurrentLoad { get; set; }

        /// <summary>
        /// Navigation property for the parent shelf.
        /// </summary>
        [ForeignKey(nameof(ShelfId))]
        public Shelf Shelf { get; set; } = null!;

        /// <summary>
        /// Gets the collection of books placed on this shelf level.
        /// </summary>
        public ICollection<Book> Books { get; set; } = new List<Book>();
    }
}
