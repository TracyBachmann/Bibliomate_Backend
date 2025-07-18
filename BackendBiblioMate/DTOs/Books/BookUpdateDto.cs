﻿using System.ComponentModel.DataAnnotations;

namespace BackendBiblioMate.DTOs
{
    /// <summary>
    /// DTO used to update an existing book.
    /// Contains fields that can be modified on a book record.
    /// </summary>
    public class BookUpdateDto
    {
        /// <summary>
        /// Gets the unique identifier of the book to update.
        /// </summary>
        /// <example>42</example>
        [Required(ErrorMessage = "BookId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "BookId must be a positive integer.")]
        public int BookId { get; init; }

        /// <summary>
        /// Gets the updated title of the book.
        /// </summary>
        /// <remarks>
        /// Must be between 1 and 200 characters.
        /// </remarks>
        /// <example>The Hobbit: Revised Edition</example>
        [Required(ErrorMessage = "Book title is required.")]
        [MinLength(1, ErrorMessage = "Title must be at least 1 character long.")]
        [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
        public string Title { get; init; } = string.Empty;

        /// <summary>
        /// Gets the updated International Standard Book Number (ISBN).
        /// </summary>
        /// <remarks>
        /// Must be between 10 and 13 characters.
        /// </remarks>
        /// <example>9780261102217</example>
        [Required(ErrorMessage = "ISBN is required.")]
        [MinLength(10, ErrorMessage = "ISBN must be at least 10 characters long.")]
        [MaxLength(13, ErrorMessage = "ISBN cannot exceed 13 characters.")]
        public string Isbn { get; init; } = string.Empty;

        /// <summary>
        /// Gets the updated publication date of the book.
        /// </summary>
        /// <example>1937-09-21</example>
        [Required(ErrorMessage = "Publication date is required.")]
        [DataType(DataType.Date, ErrorMessage = "PublicationDate must be a valid date.")]
        public DateTime PublicationDate { get; init; }

        /// <summary>
        /// Gets the updated author identifier.
        /// </summary>
        /// <example>1</example>
        [Required(ErrorMessage = "AuthorId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "AuthorId must be a positive integer.")]
        public int AuthorId { get; init; }

        /// <summary>
        /// Gets the updated genre identifier.
        /// </summary>
        /// <example>2</example>
        [Required(ErrorMessage = "GenreId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "GenreId must be a positive integer.")]
        public int GenreId { get; init; }

        /// <summary>
        /// Gets the updated editor identifier.
        /// </summary>
        /// <example>3</example>
        [Required(ErrorMessage = "EditorId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "EditorId must be a positive integer.")]
        public int EditorId { get; init; }

        /// <summary>
        /// Gets the updated shelf level identifier where the book is located.
        /// </summary>
        /// <example>5</example>
        [Required(ErrorMessage = "ShelfLevelId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "ShelfLevelId must be a positive integer.")]
        public int ShelfLevelId { get; init; }

        /// <summary>
        /// Gets the updated URL of the book’s cover image.
        /// </summary>
        /// <example>https://example.com/covers/the-hobbit-revised.jpg</example>
        [Url(ErrorMessage = "CoverUrl must be a valid URL.")]
        public string? CoverUrl { get; init; }

        /// <summary>
        /// Gets the updated list of tag identifiers associated with the book.
        /// </summary>
        /// <remarks>
        /// If specified, the list must contain at least one element.
        /// </remarks>
        /// <example>[4, 7, 12]</example>
        [MinLength(1, ErrorMessage = "If specified, TagIds must contain at least one element.")]
        public IList<int>? TagIds { get; init; } = new List<int>();
    }
}