using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    /// <summary>
    /// Represents a book author.
    /// </summary>
    public class Author
    {
        /// <summary>
        /// Primary key of the author.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AuthorId { get; set; }

        /// <summary>
        /// Full name of the author.
        /// </summary>
        [Required(ErrorMessage = "Author name is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Author name must be between 2 and 100 characters.")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Collection of books written by this author.
        /// </summary>
        public ICollection<Book> Books { get; set; } = new List<Book>();
    }
}