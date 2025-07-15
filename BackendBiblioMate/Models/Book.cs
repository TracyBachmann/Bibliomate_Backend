// BackendBiblioMate/Models/Book.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendBiblioMate.Models
{
    /// <summary>
    /// Represents a book in the library catalog, including its metadata,
    /// relationships to other entities, and availability information.
    /// </summary>
    public class Book
    {
        /// <summary>
        /// Gets or sets the primary key of the book.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BookId { get; set; }

        /// <summary>
        /// Gets or sets the title of the book.
        /// </summary>
        [Required(ErrorMessage = "Book title is required.")]
        [StringLength(255, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 255 characters.")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the International Standard Book Number (ISBN).
        /// </summary>
        [Required(ErrorMessage = "ISBN is required.")]
        [StringLength(13, MinimumLength = 10, ErrorMessage = "ISBN must be between 10 and 13 characters.")]
        public string Isbn { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the description or synopsis of the book.
        /// </summary>
        [StringLength(4000, ErrorMessage = "Description cannot exceed 4000 characters.")]
        public string? Description { get; set; }
        
        /// <summary>
        /// Gets or sets the date when the book was published.
        /// </summary>
        [Required(ErrorMessage = "Publication date is required.")]
        public DateTime PublicationDate { get; set; }

        /// <summary>
        /// Gets or sets the foreign key referencing the author of the book.
        /// </summary>
        [Required(ErrorMessage = "AuthorId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "AuthorId must be a positive integer.")]
        public int AuthorId { get; set; }

        /// <summary>
        /// Gets or sets the navigation property for the author.
        /// </summary>
        [ForeignKey(nameof(AuthorId))]
        public Author Author { get; set; } = null!;

        /// <summary>
        /// Gets or sets the foreign key referencing the genre of the book.
        /// </summary>
        [Required(ErrorMessage = "GenreId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "GenreId must be a positive integer.")]
        public int GenreId { get; set; }

        /// <summary>
        /// Gets or sets the navigation property for the genre.
        /// </summary>
        [ForeignKey(nameof(GenreId))]
        public Genre Genre { get; set; } = null!;

        /// <summary>
        /// Gets or sets the foreign key referencing the editor/publisher.
        /// </summary>
        [Required(ErrorMessage = "EditorId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "EditorId must be a positive integer.")]
        public int EditorId { get; set; }

        /// <summary>
        /// Gets or sets the navigation property for the editor/publisher.
        /// </summary>
        [ForeignKey(nameof(EditorId))]
        public Editor Editor { get; set; } = null!;

        /// <summary>
        /// Gets or sets the foreign key referencing the shelf level where the book is located.
        /// </summary>
        [Required(ErrorMessage = "ShelfLevelId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "ShelfLevelId must be a positive integer.")]
        public int ShelfLevelId { get; set; }

        /// <summary>
        /// Gets or sets the navigation property for the shelf level.
        /// </summary>
        [ForeignKey(nameof(ShelfLevelId))]
        public ShelfLevel ShelfLevel { get; set; } = null!;

        /// <summary>
        /// Gets or sets the URL of the book’s cover image.
        /// </summary>
        [Url(ErrorMessage = "CoverUrl must be a valid URL.")]
        [StringLength(2048, ErrorMessage = "CoverUrl cannot exceed 2048 characters.")]
        public string? CoverUrl { get; set; }

        /// <summary>
        /// Gets the collection of loan records associated with this book.
        /// </summary>
        public ICollection<Loan> Loans { get; set; } = new List<Loan>();

        /// <summary>
        /// Gets the collection of reservation records associated with this book.
        /// </summary>
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

        /// <summary>
        /// Gets the junction table entries linking this book to its tags.
        /// </summary>
        public ICollection<BookTag> BookTags { get; set; } = new List<BookTag>();

        /// <summary>
        /// Gets or sets the stock entry associated with this book (nullable if not yet initialized).
        /// </summary>
        public Stock? Stock { get; set; }
    }
}