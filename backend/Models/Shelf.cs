using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    /// <summary>
    /// Represents a shelf in a specific zone and genre section of the library.
    /// </summary>
    public class Shelf
    {
        /// <summary>
        /// Primary key of the shelf.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ShelfId { get; set; }

        /// <summary>
        /// Identifier of the zone where this shelf is located.
        /// </summary>
        [Required(ErrorMessage = "ZoneId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "ZoneId must be a positive integer.")]
        public int ZoneId { get; set; }

        /// <summary>
        /// Identifier of the genre associated with this shelf.
        /// </summary>
        [Required(ErrorMessage = "GenreId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "GenreId must be a positive integer.")]
        public int GenreId { get; set; }

        /// <summary>
        /// Name of the shelf.
        /// </summary>
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Maximum number of books the shelf can hold.
        /// </summary>
        [Required(ErrorMessage = "Capacity is required.")]
        [Range(0, int.MaxValue, ErrorMessage = "Capacity must be zero or a positive integer.")]
        public int Capacity { get; set; }

        /// <summary>
        /// Current number of books stored on the shelf.
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "CurrentLoad must be zero or a positive integer.")]
        public int CurrentLoad { get; set; }

        /// <summary>
        /// Navigation property for the zone containing this shelf.
        /// </summary>
        [ForeignKey(nameof(ZoneId))]
        public Zone Zone { get; set; } = null!;

        /// <summary>
        /// Navigation property for the genre assigned to this shelf.
        /// </summary>
        [ForeignKey(nameof(GenreId))]
        public Genre Genre { get; set; } = null!;

        /// <summary>
        /// Collection of levels within this shelf.
        /// </summary>
        public ICollection<ShelfLevel> ShelfLevels { get; set; } = new List<ShelfLevel>();
    }
}
