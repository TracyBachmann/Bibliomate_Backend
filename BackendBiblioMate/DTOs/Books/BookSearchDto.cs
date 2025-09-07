using System.ComponentModel.DataAnnotations;

namespace BackendBiblioMate.DTOs
{
    /// <summary>
    /// DTO used for advanced filtering when searching for books.
    /// All fields optional.
    /// </summary>
    public class BookSearchDto
    {
        [MaxLength(200)] public string? Title { get; init; }
        [MaxLength(100)] public string? Author { get; init; }
        [MaxLength(100)] public string? Publisher { get; init; }
        [MaxLength(50)]  public string? Genre { get; init; }

        [MaxLength(13)]  public string? Isbn { get; init; }

        [Range(0, 2100)] public int? YearMin { get; init; }
        [Range(0, 2100)] public int? YearMax { get; init; }

        public bool? IsAvailable { get; init; }

        public IList<int>?    TagIds   { get; init; }
        public IList<string>? TagNames { get; init; }

        [MaxLength(4000)] public string? Description { get; init; }
        [MaxLength(400)]  public string? Exclude     { get; init; }
    }
}