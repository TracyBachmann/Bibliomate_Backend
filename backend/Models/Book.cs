using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    /// <summary>
    /// Represents a book in the library catalog.
    /// </summary>
    public class Book
    {
        /// <summary>
        /// Primary key of the book.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BookId { get; set; }

        /// <summary>
        /// Title of the book.
        /// </summary>
        [Required(ErrorMessage = "Book title is required.")]
        [StringLength(255, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 255 characters.")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// International Standard Book Number (ISBN).
        /// </summary>
        [Required(ErrorMessage = "ISBN is required.")]
        [StringLength(13, MinimumLength = 10, ErrorMessage = "ISBN must be between 10 and 13 characters.")]
        public string Isbn { get; set; } = string.Empty;

        /// <summary>
        /// Date when the book was published.
        /// </summary>
        [Required(ErrorMessage = "Publication date is required.")]
        public DateTime PublicationDate { get; set; }

        /// <summary>
        /// Foreign key referencing the author of the book.
        /// </summary>
        [Required(ErrorMessage = "AuthorId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "AuthorId must be a positive integer.")]
        public int AuthorId { get; set; }

        /// <summary>
        /// Navigation property for the author.
        /// </summary>
        [ForeignKey(nameof(AuthorId))]
        public Author Author { get; set; } = null!;

        /// <summary>
        /// Foreign key referencing the genre of the book.
        /// </summary>
        [Required(ErrorMessage = "GenreId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "GenreId must be a positive integer.")]
        public int GenreId { get; set; }

        /// <summary>
        /// Navigation property for the genre.
        /// </summary>
        [ForeignKey(nameof(GenreId))]
        public Genre Genre { get; set; } = null!;

        /// <summary>
        /// Foreign key referencing the editor/publisher.
        /// </summary>
        [Required(ErrorMessage = "EditorId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "EditorId must be a positive integer.")]
        public int EditorId { get; set; }

        /// <summary>
        /// Navigation property for the editor/publisher.
        /// </summary>
        [ForeignKey(nameof(EditorId))]
        public Editor Editor { get; set; } = null!;

        /// <summary>
        /// Foreign key referencing the shelf level where the book is located.
        /// </summary>
        [Required(ErrorMessage = "ShelfLevelId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "ShelfLevelId must be a positive integer.")]
        public int ShelfLevelId { get; set; }

        /// <summary>
        /// Navigation property for the shelf level.
        /// </summary>
        [ForeignKey(nameof(ShelfLevelId))]
        public ShelfLevel ShelfLevel { get; set; } = null!;

        /// <summary>
        /// URL of the book’s cover image.
        /// </summary>
        [Url(ErrorMessage = "CoverUrl must be a valid URL.")]
        [StringLength(2048, ErrorMessage = "CoverUrl cannot exceed 2048 characters.")]
        public string? CoverUrl { get; set; }

        /// <summary>
        /// Collection of loan records associated with this book.
        /// </summary>
        public ICollection<Loan> Loans { get; set; } = new List<Loan>();

        /// <summary>
        /// Collection of reservation records associated with this book.
        /// </summary>
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

        /// <summary>
        /// Junction table entries linking this book to its tags.
        /// </summary>
        public ICollection<BookTag> BookTags { get; set; } = new List<BookTag>();

        /// <summary>
        /// Stock entry associated with this book (nullable if not yet initialized).
        /// </summary>
        public Stock? Stock { get; set; }
    }
}
