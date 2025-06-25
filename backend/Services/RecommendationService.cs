using backend.Data;
using backend.DTOs;
using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    /// <summary>
    /// Service that generates book recommendations for users
    /// based on their preferred genres.
    /// </summary>
    public class RecommendationService : IRecommendationService
    {
        private readonly BiblioMateDbContext _context;

        /// <summary>
        /// Constructs a <see cref="RecommendationService"/> with the specified database context.
        /// </summary>
        /// <param name="context">The application's EF Core database context.</param>
        public RecommendationService(BiblioMateDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc/>
        public async Task<List<RecommendationReadDto>> GetRecommendationsForUserAsync(int userId)
        {
            // 1) Query the user's preferred genre IDs
            var genreIdsQuery = _context.UserGenres
                .Where(ug => ug.UserId == userId)
                .Select(ug => ug.GenreId);

            // 2) Fetch up to 10 books matching those genres, projecting directly to DTO
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
                    CoverUrl = b.CoverUrl ?? string.Empty
                })
                .Take(10)
                .ToListAsync();

            return recommendations;
        }
    }
}