using backend.Data;
using backend.DTOs;
using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    /// <summary>
    /// Service that generates book recommendations for users
    /// based on their preferred genres.
    /// </summary>
    public class RecommendationService
    {
        private readonly BiblioMateDbContext _context;

        /// <summary>
        /// Constructs a RecommendationService with the specified database context.
        /// </summary>
        /// <param name="context">The application's EF Core database context.</param>
        public RecommendationService(BiblioMateDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves a list of recommended books for the given user,
        /// selecting up to 10 titles that match the user's favorite genres.
        /// </summary>
        /// <param name="userId">The ID of the user to get recommendations for.</param>
        /// <returns>
        /// A list of <see cref="RecommendationReadDto"/> objects containing book details.
        /// </returns>
        public async Task<List<RecommendationReadDto>> GetRecommendationsForUser(int userId)
        {
            // Query the user's preferred genre IDs
            var genreIdsQuery = _context.UserGenres
                .Where(ug => ug.UserId == userId)
                .Select(ug => ug.GenreId);

            // Fetch up to 10 books matching those genres, including author and genre names
            var recommendations = await _context.Books
                .Where(b => genreIdsQuery.Contains(b.GenreId))
                .Include(b => b.Genre)
                .Include(b => b.Author)
                .Select(b => new RecommendationReadDto
                {
                    BookId   = b.BookId,
                    Title    = b.Title,
                    Genre    = b.Genre.Name,
                    Author   = b.Author.Name,
                    CoverUrl = b.CoverUrl
                })
                .Take(10)
                .ToListAsync();

            return recommendations;
        }
    }
}
