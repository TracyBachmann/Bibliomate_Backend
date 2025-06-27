namespace backend.DTOs
{
    /// <summary>
    /// DTO returned when retrieving shelf level information.
    /// </summary>
    public class ShelfLevelReadDto
    {
        /// <summary>
        /// Unique identifier of the shelf level.
        /// </summary>
        /// <example>5</example>
        public int ShelfLevelId { get; set; }

        /// <summary>
        /// Numeric level on the shelf (e.g., 1 for bottom).
        /// </summary>
        /// <example>1</example>
        public int LevelNumber { get; set; }

        /// <summary>
        /// Identifier of the parent shelf.
        /// </summary>
        /// <example>3</example>
        public int ShelfId { get; set; }

        /// <summary>
        /// Name of the parent shelf.
        /// </summary>
        /// <example>Fantasy Shelf A</example>
        public string ShelfName { get; set; } = string.Empty;

        /// <summary>
        /// Maximum height (in cm) of books that can fit on this level.
        /// </summary>
        /// <example>30</example>
        public int MaxHeight { get; set; }

        /// <summary>
        /// Maximum number of books this level can hold.
        /// </summary>
        /// <example>20</example>
        public int Capacity { get; set; }

        /// <summary>
        /// Current number of books stored on this level.
        /// </summary>
        /// <example>12</example>
        public int CurrentLoad { get; set; }
    }
}