using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendBiblioMate.Models
{
    /// <summary>
    /// Represents a physical zone in the library (e.g., floor and aisle),
    /// grouping shelves by location and thematic area.
    /// </summary>
    public class Zone
    {
        /// <summary>
        /// Gets the primary key of the zone.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ZoneId { get; set; }

        /// <summary>
        /// Gets or sets the human-readable name of the zone.
        /// </summary>
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 100 characters.")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the floor number where the zone is located.
        /// </summary>
        [Required(ErrorMessage = "FloorNumber is required.")]
        [Range(0, int.MaxValue, ErrorMessage = "FloorNumber must be zero or a positive integer.")]
        public int FloorNumber { get; set; }

        /// <summary>
        /// Gets or sets the code of the aisle for quick identification.
        /// </summary>
        [Required(ErrorMessage = "AisleCode is required.")]
        [StringLength(20, MinimumLength = 1, ErrorMessage = "AisleCode must be between 1 and 20 characters.")]
        public string AisleCode { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets an optional description of the zone (e.g., thematic sections).
        /// </summary>
        [StringLength(255, ErrorMessage = "Description cannot exceed 255 characters.")]
        public string? Description { get; set; }

        /// <summary>
        /// Gets the collection of shelves contained within this zone.
        /// </summary>
        public ICollection<Shelf> Shelves { get; set; } = new List<Shelf>();
    }
}
