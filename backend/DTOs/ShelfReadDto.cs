namespace backend.DTOs
{
    /// <summary>
    /// DTO returned when retrieving shelf information, including related zone and genre data.
    /// </summary>
    public class ShelfReadDto
    {
        /// <summary>
        /// Unique identifier of the shelf.
        /// </summary>
        /// <example>3</example>
        public int ShelfId { get; set; }

        /// <summary>
        /// Name of the shelf.
        /// </summary>
        /// <example>Fantasy Shelf A</example>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Identifier of the zone where the shelf is located.
        /// </summary>
        /// <example>2</example>
        public int ZoneId { get; set; }

        /// <summary>
        /// Name of the zone where the shelf is located.
        /// </summary>
        /// <example>Main Hall</example>
        public string ZoneName { get; set; } = string.Empty;

        /// <summary>
        /// Identifier of the genre associated with the shelf.
        /// </summary>
        /// <example>5</example>
        public int GenreId { get; set; }

        /// <summary>
        /// Name of the genre associated with the shelf.
        /// </summary>
        /// <example>Science Fiction</example>
        public string GenreName { get; set; } = string.Empty;

        /// <summary>
        /// Maximum number of books the shelf can hold.
        /// </summary>
        /// <example>50</example>
        public int Capacity { get; set; }

        /// <summary>
        /// Current number of books stored on the shelf.
        /// </summary>
        /// <example>34</example>
        public int CurrentLoad { get; set; }
    }
}