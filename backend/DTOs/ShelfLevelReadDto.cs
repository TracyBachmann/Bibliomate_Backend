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
    }
}