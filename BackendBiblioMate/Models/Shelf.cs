using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendBiblioMate.Models
{
    /// <summary>
    /// Represents a shelf in a specific zone and genre section of the library.
    /// Organizes books by location and genre.
    /// </summary>
    public class Shelf
    {
        /// <summary>
        /// Gets the unique identifier of the shelf.
        /// </summary>
        /// <example>3</example>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ShelfId { get; init; }

        /// <summary>
        /// Gets or sets the identifier of the zone where this shelf is located.
        /// </summary>
        /// <example>2</example>
        [Range(1, int.MaxValue, ErrorMessage = "ZoneId must be a positive integer.")]
        public int ZoneId { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the genre associated with this shelf.
        /// </summary>
        /// <example>5</example>
        [Range(1, int.MaxValue, ErrorMessage = "GenreId must be a positive integer.")]
        public int GenreId { get; set; }

        /// <summary>
        /// Gets or sets the human-readable name of the shelf.
        /// </summary>
        /// <remarks>
        /// Maximum length: 100 characters.
        /// </remarks>
        /// <example>Fantasy Shelf A</example>
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the maximum number of books the shelf can hold.
        /// </summary>
        /// <remarks>
        /// Zero indicates unlimited capacity.
        /// </remarks>
        /// <example>50</example>
        [Range(0, int.MaxValue, ErrorMessage = "Capacity must be zero or a positive integer.")]
        public int Capacity { get; set; }

        /// <summary>
        /// Gets or sets the current number of books stored on the shelf.
        /// </summary>
        /// <example>34</example>
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
        /// Gets the collection of levels within this shelf.
        /// </summary>
        public ICollection<ShelfLevel> ShelfLevels { get; set; } = new List<ShelfLevel>();
    }
}
