using System.ComponentModel.DataAnnotations;

namespace backend.DTOs
{
    /// <summary>
    /// DTO used to create a new shelf.
    /// </summary>
    public class ShelfCreateDto
    {
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