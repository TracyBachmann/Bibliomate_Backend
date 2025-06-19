using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public class Genre
    {
        [Key]
        public int GenreId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        public ICollection<Book> Books { get; set; } = new List<Book>();
        public ICollection<Shelf> Shelves { get; set; } = new List<Shelf>();
    }
}