using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    /// <summary>
    /// Junction entity linking books and tags (many-to-many relationship).
    /// </summary>
    public class BookTag
    {
        /// <summary>
        /// Foreign key referencing the book.
        /// </summary>
        [Required(ErrorMessage = "BookId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "BookId must be a positive integer.")]
        public int BookId { get; set; }

        /// <summary>
        /// Navigation property for the book.
        /// </summary>
        [ForeignKey(nameof(BookId))]
        public Book Book { get; set; } = null!;

        /// <summary>
        /// Foreign key referencing the tag.
        /// </summary>
        [Required(ErrorMessage = "TagId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "TagId must be a positive integer.")]
        public int TagId { get; set; }

        /// <summary>
        /// Navigation property for the tag.
        /// </summary>
        [ForeignKey(nameof(TagId))]
        public Tag Tag { get; set; } = null!;
    }
}