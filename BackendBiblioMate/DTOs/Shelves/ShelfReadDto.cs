namespace BackendBiblioMate.DTOs
{
    /// <summary>
    /// DTO returned when retrieving shelf information, including related zone and genre data.
    /// Contains all fields necessary to display shelf details and status.
    /// </summary>
    public class ShelfReadDto
    {
        /// <summary>
        /// Gets the unique identifier of the shelf.
        /// </summary>
        /// <example>3</example>
        public int ShelfId { get; init; }

        /// <summary>
        /// Gets the name of the shelf.
        /// </summary>
        /// <example>Fantasy Shelf A</example>
        public string Name { get; init; } = string.Empty;

        /// <summary>
        /// Gets the identifier of the zone where the shelf is located.
        /// </summary>
        /// <example>2</example>
        public int ZoneId { get; init; }

        /// <summary>
        /// Gets the name of the zone where the shelf is located.
        /// </summary>
        /// <example>Main Hall</example>
        public string ZoneName { get; init; } = string.Empty;

        /// <summary>
        /// Gets the identifier of the genre associated with the shelf.
        /// </summary>
        /// <example>5</example>
        public int GenreId { get; init; }

        /// <summary>
        /// Gets the name of the genre associated with the shelf.
        /// </summary>
        /// <example>Science Fiction</example>
        public string GenreName { get; init; } = string.Empty;

        /// <summary>
        /// Gets the maximum number of books the shelf can hold.
        /// </summary>
        /// <example>50</example>
        public int Capacity { get; init; }

        /// <summary>
        /// Gets the current number of books stored on the shelf.
        /// </summary>
        /// <example>34</example>
        public int CurrentLoad { get; init; }
    }
}