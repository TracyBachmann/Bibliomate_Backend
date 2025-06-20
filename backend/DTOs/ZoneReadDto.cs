namespace backend.DTOs
{
    /// <summary>
    /// DTO used to return zone data to the client.
    /// </summary>
    public class ZoneReadDto
    {
        public int ZoneId { get; set; }
        public int FloorNumber { get; set; }
        public string AisleCode { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}