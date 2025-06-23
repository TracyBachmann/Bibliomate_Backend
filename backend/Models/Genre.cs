using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    /// <summary>
    /// Represents a literary genre (e.g., Science Fiction, Romance).
    /// </summary>
    public class Genre
    {
        /// <summary>
        /// Primary key of the genre.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int GenreId { get; set; }

        /// <summary>
        /// Name of the genre.
        /// </summary>
        [Required(ErrorMessage = "Genre name is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Genre name must be between 2 and 100 characters.")]
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// Collection of books that belong to this genre.
        /// </summary>
        public ICollection<Book> Books { get; set; } = new List<Book>();
        
        /// <summary>
        /// Collection of shelves designated for this genre.
        /// </summary>
        public ICollection<Shelf> Shelves { get; set; } = new List<Shelf>();
    }
}