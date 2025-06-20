using System.ComponentModel.DataAnnotations;

namespace backend.DTOs
{
    /// <summary>
    /// DTO used to update an existing shelf level.
    /// </summary>
    public class ShelfLevelUpdateDto
    {
        [Required]
        public int ShelfLevelId { get; set; }

        [Required]
        public int LevelNumber { get; set; }

        [Required]
        public int ShelfId { get; set; }
    }
}