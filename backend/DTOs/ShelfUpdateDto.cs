using System.ComponentModel.DataAnnotations;

namespace backend.DTOs
{
    /// <summary>
    /// DTO used to update an existing shelf.
    /// </summary>
    public class ShelfUpdateDto
    {
        [Required]
        public int ShelfId { get; set; }

        [Required]
        public int ZoneId { get; set; }

        [Required]
        public int GenreId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Range(0, int.MaxValue)]
        public int Capacity { get; set; }
    }
}