using backend.DTOs;
using backend.Models;
using backend.Data;
using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    /// <summary>
    /// Service that generates book recommendations for users based on their preferred genres.
    /// </summary>
    public class RecommendationService
    {
        private readonly BiblioMateDbContext _context;

        /// <summary>
        /// Constructs a RecommendationService with the specified database context.
        /// </summary>
        /// <param name="context">The application's database context.</param>
        public RecommendationService(BiblioMateDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets a list of recommended books for a given user, based on their preferred genres.
        /// </summary>
        /// <param name="userId">The ID of the user to get recommendations for.</param>
        /// <returns>A list of recommended books.</returns>
        public async Task<List<RecommendationReadDto>> GetRecommendationsForUser(int userId)
        {
            // Prépare une requête sur les genres favoris de l'utilisateur
            var genreIdsQuery = _context.UserGenres
                .Where(ug => ug.UserId == userId)
                .Select(ug => ug.GenreId);

            // Récupère les livres correspondant à ces genres
            var books = await _context.Books
                .Where(b => genreIdsQuery.Contains(b.GenreId))
                .Include(b => b.Genre)
                .Include(b => b.Author)
                .Select(b => new RecommendationReadDto
                {
                    BookId = b.BookId,
                    Title = b.Title,
                    Genre = b.Genre.Name,
                    Author = b.Author.Name,
                    CoverUrl = b.CoverUrl
                })
                .Take(10)
                .ToListAsync();

            return books;
        }
    }
}