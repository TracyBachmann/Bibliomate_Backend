using System.ComponentModel.DataAnnotations;

namespace backend.DTOs
{
    /// <summary>
    /// DTO used to update an existing book.
    /// </summary>
    public class BookUpdateDto
    {
        /// <summary>
        /// Unique identifier of the book to update.
        /// </summary>
        /// <example>42</example>
        [Required(ErrorMessage = "BookId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "BookId must be a positive integer.")]
        public int BookId { get; set; }

        /// <summary>
        /// Updated title of the book.
        /// </summary>
        /// <example>The Hobbit: Revised Edition</example>
        [Required(ErrorMessage = "Book title is required.")]
        [StringLength(200, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 200 characters.")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Updated International Standard Book Number (ISBN).
        /// </summary>
        /// <example>9780261102217</example>
        [Required(ErrorMessage = "ISBN is required.")]
        [StringLength(13, MinimumLength = 10, ErrorMessage = "ISBN must be between 10 and 13 characters.")]
        public string Isbn { get; set; } = string.Empty;

        /// <summary>
        /// Updated publication date of the book.
        /// </summary>
        /// <example>1937-09-21</example>
        [Required(ErrorMessage = "Publication date is required.")]
        [DataType(DataType.Date, ErrorMessage = "PublicationDate must be a valid date.")]
        public DateTime PublicationDate { get; set; }

        /// <summary>
        /// Updated author identifier.
        /// </summary>
        /// <example>1</example>
        [Required(ErrorMessage = "AuthorId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "AuthorId must be a positive integer.")]
        public int AuthorId { get; set; }

        /// <summary>
        /// Updated genre identifier.
        /// </summary>
        /// <example>2</example>
        [Required(ErrorMessage = "GenreId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "GenreId must be a positive integer.")]
        public int GenreId { get; set; }

        /// <summary>
        /// Updated editor identifier.
        /// </summary>
        /// <example>3</example>
        [Required(ErrorMessage = "EditorId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "EditorId must be a positive integer.")]
        public int EditorId { get; set; }

        /// <summary>
        /// Updated shelf level identifier where the book is located.
        /// </summary>
        /// <example>5</example>
        [Required(ErrorMessage = "ShelfLevelId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "ShelfLevelId must be a positive integer.")]
        public int ShelfLevelId { get; set; }
        
        /// <summary>
        /// Updated URL of the book’s cover image.
        /// </summary>
        /// <example>https://example.com/covers/the-hobbit-revised.jpg</example>
        [Url(ErrorMessage = "CoverUrl must be a valid URL.")]
        public string? CoverUrl { get; set; }

        /// <summary>
        /// Updated list of tag identifiers associated with the book.
        /// </summary>
        /// <example>[4, 7, 12]</example>
        [MinLength(1, ErrorMessage = "If specified, TagIds must contain at least one element.")]
        public List<int>? TagIds { get; set; }
    }
}
