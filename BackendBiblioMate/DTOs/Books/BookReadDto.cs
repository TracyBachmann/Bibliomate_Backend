namespace BackendBiblioMate.DTOs
{
    public sealed class BookReadDto
    {
        public int    BookId { get; set; }
        public string Title  { get; set; } = "";
        public string Isbn   { get; set; } = "";
        public int    PublicationYear { get; set; }
        public string AuthorName { get; set; } = "";
        public string GenreName  { get; set; } = "";
        public string EditorName { get; set; } = "";

        // ← Calculé côté service : Quantity - prêts actifs > 0
        public bool   IsAvailable { get; set; }

        public string? CoverUrl { get; set; }
        public string? Description { get; set; }
        public List<string> Tags { get; set; } = new();

        // Localisation
        public int?    Floor { get; set; }
        public string? Aisle { get; set; }
        public string? Rayon { get; set; }
        public int?    Shelf { get; set; }
    }
}