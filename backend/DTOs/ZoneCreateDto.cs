namespace backend.DTOs
{
    /// <summary>
    /// DTO used to create a new zone.
    /// </summary>
    public class ZoneCreateDto
    {
        public int FloorNumber { get; set; }
        public string AisleCode { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}