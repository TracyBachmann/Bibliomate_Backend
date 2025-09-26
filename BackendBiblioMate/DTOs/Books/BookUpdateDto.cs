using System.ComponentModel.DataAnnotations;

namespace BackendBiblioMate.DTOs
{
    /// <summary>
    /// Data Transfer Object used to update an existing book record.
    /// </summary>
    public class BookUpdateDto
    {
        /// <summary>Gets or sets the unique identifier of the book.</summary>
        [Required(ErrorMessage = "BookId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "BookId must be a positive integer.")]
        public int BookId { get; init; }

        /// <summary>Gets or sets the title of the book.</summary>
        [Required(ErrorMessage = "Book title is required.")]
        [MinLength(1, ErrorMessage = "Title must be at least 1 character long.")]
        [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
        public string Title { get; init; } = string.Empty;

        /// <summary>Gets or sets the ISBN of the book.</summary>
        [Required(ErrorMessage = "ISBN is required.")]
        [MinLength(10, ErrorMessage = "ISBN must be at least 10 characters long.")]
        [MaxLength(13, ErrorMessage = "ISBN cannot exceed 13 characters.")]
        public string Isbn { get; init; } = string.Empty;

        /// <summary>Gets or sets the description of the book.</summary>
        [MaxLength(4000, ErrorMessage = "Description cannot exceed 4000 characters.")]
        public string? Description { get; init; }

        /// <summary>Gets or sets the publication date of the book.</summary>
        [Required(ErrorMessage = "Publication date is required.")]
        [DataType(DataType.Date, ErrorMessage = "PublicationDate must be a valid date.")]
        public DateTime PublicationDate { get; init; }

        /// <summary>Gets or sets the identifier of the author.</summary>
        [Required(ErrorMessage = "AuthorId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "AuthorId must be a positive integer.")]
        public int AuthorId { get; init; }

        /// <summary>Gets or sets the identifier of the genre.</summary>
        [Required(ErrorMessage = "GenreId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "GenreId must be a positive integer.")]
        public int GenreId { get; init; }

        /// <summary>Gets or sets the identifier of the editor.</summary>
        [Required(ErrorMessage = "EditorId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "EditorId must be a positive integer.")]
        public int EditorId { get; init; }

        /// <summary>Gets or sets the identifier of the shelf level.</summary>
        [Required(ErrorMessage = "ShelfLevelId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "ShelfLevelId must be a positive integer.")]
        public int ShelfLevelId { get; init; }

        /// <summary>Gets or sets the URL of the book’s cover image.</summary>
        [Url(ErrorMessage = "CoverUrl must be a valid URL.")]
        public string? CoverUrl { get; init; }

        /// <summary>Gets or sets the list of tag identifiers associated with the book.</summary>
        [MinLength(1, ErrorMessage = "If specified, TagIds must contain at least one element.")]
        public IList<int>? TagIds { get; init; } = new List<int>();

        /// <summary>
        /// Gets or sets the optional stock quantity of the book.
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "StockQuantity must be zero or a positive integer.")]
        public int? StockQuantity { get; init; }
    }
}