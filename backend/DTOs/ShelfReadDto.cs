namespace backend.DTOs
{
    /// <summary>
    /// DTO used to read shelf information with related zone and genre names.
    /// </summary>
    public class ShelfReadDto
    {
        public int ShelfId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int ZoneId { get; set; }
        public string ZoneName { get; set; } = string.Empty;
        public int GenreId { get; set; }
        public string GenreName { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public int CurrentLoad { get; set; }
    }
}