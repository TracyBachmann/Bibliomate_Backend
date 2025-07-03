using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendBiblioMate.Models
{
    /// <summary>
    /// Represents a tag that can be associated with multiple books.
    /// Used for categorization and search filtering.
    /// </summary>
    public class Tag
    {
        /// <summary>
        /// Gets the primary key of the tag.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TagId { get; init; }

        /// <summary>
        /// Gets or sets the name of the tag.
        /// </summary>
        [Required(ErrorMessage = "Tag name is required.")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Tag name must be between 1 and 50 characters.")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets the junction collection linking this tag to books.
        /// </summary>
        public ICollection<BookTag> BookTags { get; init; } = new List<BookTag>();
    }
}