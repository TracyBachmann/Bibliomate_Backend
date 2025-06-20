namespace backend.DTOs
{
    /// <summary>
    /// DTO used to update an existing zone.
    /// </summary>
    public class ZoneUpdateDto
    {
        public int ZoneId { get; set; }
        public int FloorNumber { get; set; }
        public string AisleCode { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}