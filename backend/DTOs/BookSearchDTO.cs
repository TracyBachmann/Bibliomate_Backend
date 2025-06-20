using System.ComponentModel.DataAnnotations;

namespace backend.Models.DTOs
{
    /// <summary>
    /// DTO used for advanced search filtering of books.
    /// </summary>
    public class BookSearchDTO
    {
        public string? Title { get; set; }
        public string? Author { get; set; }
        public string? Publisher { get; set; }
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