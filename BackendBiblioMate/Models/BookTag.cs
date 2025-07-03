using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendBiblioMate.Models
{
    /// <summary>
    /// Junction entity linking books and tags (many-to-many relationship).
    /// </summary>
    public class BookTag
    {
        /// <summary>
        /// Gets or sets the foreign key of the associated book.
        /// </summary>
        [Required(ErrorMessage = "BookId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "BookId must be a positive integer.")]
        public int BookId { get; init; }

        /// <summary>
        /// Gets or sets the navigation property for the book.
        /// </summary>
        [ForeignKey(nameof(BookId))]
        public Book Book { get; init; } = null!;

        /// <summary>
        /// Gets or sets the foreign key of the associated tag.
        /// </summary>
        [Required(ErrorMessage = "TagId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "TagId must be a positive integer.")]
        public int TagId { get; init; }

        /// <summary>
        /// Gets or sets the navigation property for the tag.
        /// </summary>
        [ForeignKey(nameof(TagId))]
        public Tag Tag { get; init; } = null!;
    }
}