using System;

namespace backend.DTOs
{
    /// <summary>
    /// DTO used to read recommended books for a user.
    /// </summary>
    public class RecommendationReadDto
    {

        public int BookId { get; set; }

        public string Title { get; set; }

        public string Genre { get; set; }

        public string Author { get; set; }

        public string CoverUrl { get; set; }
    }
}