using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendBiblioMate.Models
{
    /// <summary>
    /// Represents an author of a book in the system.
    /// Contains basic identification and navigation to related books.
    /// </summary>
    public class Author
    {
        /// <summary>
        /// Gets or sets the primary key of the author.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AuthorId { get; init; }

        /// <summary>
        /// Gets or sets the full name of the author.
        /// </summary>
        [Required(ErrorMessage = "Author name is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Author name must be between 2 and 100 characters.")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets the collection of books written by this author.
        /// </summary>
        public ICollection<Book> Books { get; init; } = new List<Book>();
    }
}