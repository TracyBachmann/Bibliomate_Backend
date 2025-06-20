using System.ComponentModel.DataAnnotations;

namespace backend.DTOs
{
    public class BookSearchDto
    {
        [StringLength(255)]
        public string? Title { get; set; }

        [StringLength(100)]
        public string? Author { get; set; }

        [StringLength(100)]
        public string? Publisher { get; set; }

        [StringLength(100)]
        public string? Genre { get; set; }

        [StringLength(13, MinimumLength = 10)]
        public string? Isbn { get; set; }

        [Range(0, 2100)]
        public int? YearMin { get; set; }

        [Range(0, 2100)]
        public int? YearMax { get; set; }

        public bool? IsAvailable { get; set; }

        public List<int>? TagIds { get; set; }
    }
}