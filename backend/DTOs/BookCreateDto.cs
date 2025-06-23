using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace backend.DTOs
{
    /// <summary>
    /// DTO used to create a new book entry.
    /// </summary>
    public class BookCreateDto
    {
        /// <summary>
        /// Title of the book.
        /// </summary>
        /// <example>The Hobbit</example>
        [Required(ErrorMessage = "Book title is required.")]
        [StringLength(200, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 200 characters.")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// International Standard Book Number (ISBN).
        /// </summary>
        /// <example>9780261103344</example>
        [Required(ErrorMessage = "ISBN is required.")]
        [StringLength(13, MinimumLength = 10, ErrorMessage = "ISBN must be between 10 and 13 characters.")]
        public string Isbn { get; set; } = string.Empty;

        /// <summary>
        /// Publication date of the book.
        /// </summary>
        /// <example>1937-09-21</example>
        [Required(ErrorMessage = "Publication date is required.")]
        [DataType(DataType.Date, ErrorMessage = "PublicationDate must be a valid date.")]
        public DateTime PublicationDate { get; set; }

        /// <summary>
        /// Identifier of the author.
        /// </summary>
        /// <example>1</example>
        [Required(ErrorMessage = "AuthorId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "AuthorId must be a positive integer.")]
        public int AuthorId { get; set; }

        /// <summary>
        /// Identifier of the genre.
        /// </summary>
        /// <example>2</example>
        [Required(ErrorMessage = "GenreId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "GenreId must be a positive integer.")]
        public int GenreId { get; set; }

        /// <summary>
        /// Identifier of the editor.
        /// </summary>
        /// <example>3</example>
        [Required(ErrorMessage = "EditorId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "EditorId must be a positive integer.")]
        public int EditorId { get; set; }

        /// <summary>
        /// Identifier of the shelf level where the book is located.
        /// </summary>
        /// <example>5</example>
        [Required(ErrorMessage = "ShelfLevelId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "ShelfLevelId must be a positive integer.")]
        public int ShelfLevelId { get; set; }
        
        /// <summary>
        /// URL of the book’s cover image.
        /// </summary>
        /// <example>https://example.com/covers/the-hobbit.jpg</example>
        [Url(ErrorMessage = "CoverUrl must be a valid URL.")]
        public string? CoverUrl { get; set; }

        /// <summary>
        /// List of tag identifiers associated with the book.
        /// </summary>
        /// <example>[4, 7, 12]</example>
        [MinLength(1, ErrorMessage = "If specified, TagIds must contain at least one element.")]
        public List<int>? TagIds { get; set; }
    }
}
