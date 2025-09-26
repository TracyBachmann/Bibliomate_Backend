namespace BackendBiblioMate.DTOs
{
    /// <summary>
    /// Data Transfer Object returned when retrieving book information.
    /// Contains metadata, availability and location.
    /// </summary>
    public sealed class BookReadDto
    {
        /// <summary>Gets or sets the unique identifier of the book.</summary>
        public int BookId { get; set; }

        /// <summary>Gets or sets the title of the book.</summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>Gets or sets the ISBN of the book.</summary>
        public string Isbn { get; set; } = string.Empty;

        /// <summary>Gets or sets the year of publication.</summary>
        public int PublicationYear { get; set; }

        /// <summary>Gets or sets the name of the author.</summary>
        public string AuthorName { get; set; } = string.Empty;

        /// <summary>Gets or sets the genre name.</summary>
        public string GenreName { get; set; } = string.Empty;

        /// <summary>Gets or sets the editor/publisher name.</summary>
        public string EditorName { get; set; } = string.Empty;

        /// <summary>Gets or sets whether the book is available in stock.</summary>
        public bool IsAvailable { get; set; }

        /// <summary>Gets or sets the current stock quantity.</summary>
        public int StockQuantity { get; set; }

        /// <summary>Gets or sets the URL of the book’s cover image.</summary>
        public string? CoverUrl { get; set; }

        /// <summary>Gets or sets the description of the book.</summary>
        public string? Description { get; set; }

        /// <summary>Gets or sets the list of tag names associated with the book.</summary>
        public List<string> Tags { get; set; } = new();

        /// <summary>Gets or sets the floor where the book is located.</summary>
        public int? Floor { get; set; }

        /// <summary>Gets or sets the aisle identifier.</summary>
        public string? Aisle { get; set; }

        /// <summary>Gets or sets the rayon/section name.</summary>
        public string? Rayon { get; set; }

        /// <summary>Gets or sets the shelf number.</summary>
        public int? Shelf { get; set; }
    }
}