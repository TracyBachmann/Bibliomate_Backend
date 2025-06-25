using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    /// <summary>
    /// Represents a tag that can be associated with multiple books.
    /// </summary>
    public class Tag
    {
        /// <summary>
        /// Primary key of the tag.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TagId { get; set; }

        /// <summary>
        /// Name of the tag.
        /// </summary>
        [Required(ErrorMessage = "Tag name is required.")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Tag name must be between 1 and 50 characters.")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Junction collection linking this tag to books.
        /// </summary>
        public ICollection<BookTag> BookTags { get; set; } = new List<BookTag>();
    }
}