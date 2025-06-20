using System.ComponentModel.DataAnnotations;

namespace backend.DTOs
{
    /// <summary>
    /// DTO used to create a new shelf level.
    /// </summary>
    public class ShelfLevelCreateDto
    {
        [Required]
        public int LevelNumber { get; set; }

        [Required]
        public int ShelfId { get; set; }
    }
}