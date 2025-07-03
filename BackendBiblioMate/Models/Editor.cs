using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendBiblioMate.Models
{
    /// <summary>
    /// Represents a publisher or editor of books within the system.
    /// Contains identification and navigation to associated books.
    /// </summary>
    public class Editor
    {
        /// <summary>
        /// Gets or sets the primary key of the editor.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EditorId { get; init; }

        /// <summary>
        /// Gets or sets the name of the editor or publishing house.
        /// </summary>
        [Required(ErrorMessage = "Editor name is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Editor name must be between 2 and 100 characters.")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets the collection of books published by this editor.
        /// </summary>
        public ICollection<Book> Books { get; init; } = new List<Book>();
    }
}