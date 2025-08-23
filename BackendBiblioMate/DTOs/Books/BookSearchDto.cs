using System.ComponentModel.DataAnnotations;

namespace BackendBiblioMate.DTOs
{
    /// <summary>
    /// DTO used for advanced filtering when searching for books.
    /// Contains optional criteria to narrow down search results.
    /// </summary>
    public class BookSearchDto
    {
        /// <summary>
        /// Gets the partial or full title to search for.
        /// </summary>
        /// <example>Hobbit</example>
        [MaxLength(200, ErrorMessage = "Title filter cannot exceed 200 characters.")]
        public string? Title { get; init; }

        /// <summary>
        /// Gets the partial or full author name to filter by.
        /// </summary>
        /// <example>Tolkien</example>
        [MaxLength(100, ErrorMessage = "Author filter cannot exceed 100 characters.")]
        public string? Author { get; init; }

        /// <summary>
        /// Gets the partial or full publisher name to filter by.
        /// </summary>
        /// <example>HarperCollins</example>
        [MaxLength(100, ErrorMessage = "Publisher filter cannot exceed 100 characters.")]
        public string? Publisher { get; init; }

        /// <summary>
        /// Gets the genre name to filter by.
        /// </summary>
        /// <example>Fantasy</example>
        [MaxLength(50, ErrorMessage = "Genre filter cannot exceed 50 characters.")]
        public string? Genre { get; init; }

        /// <summary>
        /// Gets the ISBN to filter by (10 to 13 characters).
        /// </summary>
        /// <remarks>
        /// Use this to find an exact or partial match of the book’s ISBN.
        /// </remarks>
        /// <example>9780261103344</example>
        [MinLength(10, ErrorMessage = "ISBN filter must be at least 10 characters long.")]
        [MaxLength(13, ErrorMessage = "ISBN filter cannot exceed 13 characters.")]
        public string? Isbn { get; init; }

        /// <summary>
        /// Gets the minimum publication year to filter by.
        /// </summary>
        /// <remarks>
        /// Inclusive lower bound (0–2100).
        /// </remarks>
        /// <example>1900</example>
        [Range(0, 2100, ErrorMessage = "YearMin must be between 0 and 2100.")]
        public int? YearMin { get; init; }

        /// <summary>
        /// Gets the maximum publication year to filter by.
        /// </summary>
        /// <remarks>
        /// Inclusive upper bound (0–2100).
        /// </remarks>
        /// <example>2000</example>
        [Range(0, 2100, ErrorMessage = "YearMax must be between 0 and 2100.")]
        public int? YearMax { get; init; }

        /// <summary>
        /// Gets the availability status to filter by.
        /// </summary>
        /// <example>true</example>
        public bool? IsAvailable { get; init; }

        /// <summary>
        /// Gets the list of tag identifiers to filter by.
        /// </summary>
        /// <remarks>
        /// If specified, the list must contain at least one element.
        /// </remarks>
        [MinLength(1, ErrorMessage = "If specified, TagIds must contain at least one element.")]
        public IList<int> TagIds { get; init; } = new List<int>();

        /// <summary>
        /// Gets the partial or full description text to filter by.
        /// </summary>
        /// <remarks>
        /// Allows searching within the description.
        /// </remarks>
        /// <example>epic tale</example>
        [MaxLength(4000, ErrorMessage = "Description filter cannot exceed 4000 characters.")]
        public string? Description { get; init; }
    }
}
