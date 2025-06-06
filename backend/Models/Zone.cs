using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public class Zone
    {
        [Key]
        public int ZoneId { get; set; }

        [Required]
        public int FloorNumber { get; set; }

        [Required]
        [MaxLength(20)]
        public string AisleCode { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Description { get; set; }

        public ICollection<Shelf>? Shelves { get; set; }
    }
}