using System.ComponentModel.DataAnnotations;

namespace BackendBiblioMate.DTOs
{
    /// <summary>
    /// Data Transfer Object used for advanced filtering when searching for books.
    /// All fields are optional.
    /// </summary>
    public class BookSearchDto
    {
        /// <summary>Gets or sets the title filter.</summary>
        [MaxLength(200)] public string? Title { get; init; }

        /// <summary>Gets or sets the author filter.</summary>
        [MaxLength(100)] public string? Author { get; init; }

        /// <summary>Gets or sets the publisher filter.</summary>
        [MaxLength(100)] public string? Publisher { get; init; }

        /// <summary>Gets or sets the genre filter.</summary>
        [MaxLength(50)] public string? Genre { get; init; }

        /// <summary>Gets or sets the ISBN filter.</summary>
        [MaxLength(13)] public string? Isbn { get; init; }

        /// <summary>Gets or sets the minimum publication year filter.</summary>
        [Range(0, 2100)] public int? YearMin { get; init; }

        /// <summary>Gets or sets the maximum publication year filter.</summary>
        [Range(0, 2100)] public int? YearMax { get; init; }

        /// <summary>Gets or sets whether to filter by availability.</summary>
        public bool? IsAvailable { get; init; }

        /// <summary>Gets or sets the list of tag identifiers to filter by.</summary>
        public IList<int>? TagIds { get; init; }

        /// <summary>Gets or sets the list of tag names to filter by.</summary>
        public IList<string>? TagNames { get; init; }

        /// <summary>Gets or sets a description keyword filter.</summary>
        [MaxLength(4000)] public string? Description { get; init; }

        /// <summary>Gets or sets words to exclude from search results.</summary>
        [MaxLength(400)] public string? Exclude { get; init; }
    }
}