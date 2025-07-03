namespace BackendBiblioMate.DTOs
{
    /// <summary>
    /// DTO returned when retrieving shelf level information.
    /// Contains details about a specific level within a shelf.
    /// </summary>
    public class ShelfLevelReadDto
    {
        /// <summary>
        /// Gets the unique identifier of the shelf level.
        /// </summary>
        /// <example>5</example>
        public int ShelfLevelId { get; init; }

        /// <summary>
        /// Gets the numeric level on the shelf (e.g., 1 for bottom).
        /// </summary>
        /// <example>1</example>
        public int LevelNumber { get; init; }

        /// <summary>
        /// Gets the identifier of the parent shelf.
        /// </summary>
        /// <example>3</example>
        public int ShelfId { get; init; }

        /// <summary>
        /// Gets the name of the parent shelf.
        /// </summary>
        /// <example>Fantasy Shelf A</example>
        public string ShelfName { get; init; } = string.Empty;

        /// <summary>
        /// Gets the maximum height in centimeters of books that can fit on this level.
        /// </summary>
        /// <example>30</example>
        public int MaxHeight { get; init; }

        /// <summary>
        /// Gets the maximum number of books this level can hold.
        /// </summary>
        /// <example>20</example>
        public int Capacity { get; init; }

        /// <summary>
        /// Gets the current number of books stored on this level.
        /// </summary>
        /// <example>12</example>
        public int CurrentLoad { get; init; }
    }
}