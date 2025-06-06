using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class ShelfLevel
    {
        [Key]
        public int ShelfLevelId { get; set; }

        [Required]
        public int ShelfId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int LevelNumber { get; set; }

        [Range(0, int.MaxValue)]
        public int MaxHeight { get; set; }

        [Range(0, int.MaxValue)]
        public int Capacity { get; set; }

        [Range(0, int.MaxValue)]
        public int CurrentLoad { get; set; }

        [ForeignKey("ShelfId")]
        public Shelf Shelf { get; set; } = null!;

        public ICollection<Book> Books { get; set; } = new List<Book>();
    }
}