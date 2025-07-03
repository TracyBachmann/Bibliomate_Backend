using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendBiblioMate.Models
{
    /// <summary>
    /// Represents a literary genre (e.g., Science Fiction, Romance) in the system.
    /// Contains identification and navigation to related books and shelves.
    /// </summary>
    public class Genre
    {
        /// <summary>
        /// Gets or sets the primary key of the genre.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int GenreId { get; init; }

        /// <summary>
        /// Gets or sets the name of the genre.
        /// </summary>
        [Required(ErrorMessage = "Genre name is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Genre name must be between 2 and 100 characters.")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets the collection of books that belong to this genre.
        /// </summary>
        public ICollection<Book> Books { get; init; } = new List<Book>();

        /// <summary>
        /// Gets the collection of shelves designated for this genre.
        /// </summary>
        public ICollection<Shelf> Shelves { get; init; } = new List<Shelf>();
    }
}