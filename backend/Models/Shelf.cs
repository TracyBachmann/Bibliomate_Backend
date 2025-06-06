using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class Shelf
    {
        [Key]
        public int ShelfId { get; set; }

        [Required]
        public int ZoneId { get; set; }

        [Required]
        public int GenreId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Range(0, int.MaxValue)]
        public int Capacity { get; set; }

        [Range(0, int.MaxValue)]
        public int CurrentLoad { get; set; }

        [ForeignKey("ZoneId")]
        public Zone Zone { get; set; } = null!;

        [ForeignKey("GenreId")]
        public Genre Genre { get; set; } = null!;

        public ICollection<ShelfLevel> ShelfLevels { get; set; } = new List<ShelfLevel>();
    }
}