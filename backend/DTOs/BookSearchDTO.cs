public class BookSearchDto
{
    public string? Title { get; set; }
    public string? Author { get; set; }
    public string? Genre { get; set; }
    public bool? IsAvailable { get; set; }
    public int? YearMin { get; set; }
    public int? YearMax { get; set; }
    public string? Isbn { get; set; }
    public string? Publisher { get; set; }
    public List<int>? TagIds { get; set; }
}