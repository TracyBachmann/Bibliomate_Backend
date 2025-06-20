namespace backend.DTOs
{
    /// <summary>
    /// DTO used to expose shelf level information to the client.
    /// </summary>
    public class ShelfLevelReadDto
    {
        public int ShelfLevelId { get; set; }
        public int LevelNumber { get; set; }
        public int ShelfId { get; set; }
        public string ShelfName { get; set; } = string.Empty;
    }
}