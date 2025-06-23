using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace backend.DTOs
{
    /// <summary>
    /// DTO used for advanced filtering when searching for books.
    /// </summary>
    public class BookSearchDto
    {
        /// <summary>
        /// Partial or full title to search for.
        /// </summary>
        /// <example>Hobbit</example>
        [StringLength(200, ErrorMessage = "Title filter cannot exceed 200 characters.")]
        public string? Title { get; set; }

        /// <summary>
        /// Partial or full author name to filter by.
        /// </summary>
        /// <example>Tolkien</example>
        [StringLength(100, ErrorMessage = "Author filter cannot exceed 100 characters.")]
        public string? Author { get; set; }

        /// <summary>
        /// Partial or full publisher name to filter by.
        /// </summary>
        /// <example>HarperCollins</example>
        [StringLength(100, ErrorMessage = "Publisher filter cannot exceed 100 characters.")]
        public string? Publisher { get; set; }

        /// <summary>
        /// Genre name to filter by.
        /// </summary>
        /// <example>Fantasy</example>
        [StringLength(50, ErrorMessage = "Genre filter cannot exceed 50 characters.")]
        public string? Genre { get; set; }

        /// <summary>
        /// ISBN to filter by (10 to 13 characters).
        /// </summary>
        /// <example>9780261103344</example>
        [StringLength(13, MinimumLength = 10, ErrorMessage = "ISBN filter must be between 10 and 13 characters.")]
        public string? Isbn { get; set; }

        /// <summary>
        /// Minimum publication year to filter by.
        /// </summary>
        /// <example>1900</example>
        [Range(0, 2100, ErrorMessage = "YearMin must be between 0 and 2100.")]
        public int? YearMin { get; set; }

        /// <summary>
        /// Maximum publication year to filter by.
        /// </summary>
        /// <example>2000</example>
        [Range(0, 2100, ErrorMessage = "YearMax must be between 0 and 2100.")]
        public int? YearMax { get; set; }

        /// <summary>
        /// Filter by availability status.
        /// </summary>
        /// <example>true</example>
        public bool? IsAvailable { get; set; }

        /// <summary>
        /// List of tag identifiers to filter by.
        /// </summary>
        /// <example>[1, 4, 7]</example>
        [MinLength(1, ErrorMessage = "If specified, TagIds must contain at least one element.")]
        public List<int>? TagIds { get; set; }
    }
}
