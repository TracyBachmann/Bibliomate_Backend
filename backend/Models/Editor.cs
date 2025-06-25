using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    /// <summary>
    /// Represents a publisher or editor of books.
    /// </summary>
    public class Editor
    {
        /// <summary>
        /// Primary key of the editor.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EditorId { get; set; }

        /// <summary>
        /// Name of the editor or publishing house.
        /// </summary>
        [Required(ErrorMessage = "Editor name is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Editor name must be between 2 and 100 characters.")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Collection of books published by this editor.
        /// </summary>
        public ICollection<Book> Books { get; set; } = new List<Book>();
    }
}