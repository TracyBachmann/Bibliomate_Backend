using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BackendBiblioMate.Data;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Interfaces;

namespace BackendBiblioMate.Services.Recommendations
{
    /// <summary>
    /// Service that generates book recommendations for users based on their preferred genres.
    /// </summary>
    public class RecommendationService : IRecommendationService
    {
        private readonly BiblioMateDbContext _context;

        /// <summary>
        /// Initializes a new instance of <see cref="RecommendationService"/>.
        /// </summary>
        /// <param name="context">The application's EF Core database context.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is null.</exception>
        public RecommendationService(BiblioMateDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Retrieves up to 10 book recommendations for the specified user,
        /// matching the user's preferred genres.
        /// </summary>
        /// <param name="userId">Identifier of the user to recommend books for.</param>
        /// <param name="cancellationToken">Token to monitor cancellation of the operation.</param>
        /// <returns>
        /// List of <see cref="RecommendationReadDto"/> containing recommended books,
        /// ordered by <c>BookId</c> to ensure deterministic results.
        /// </returns>
        public async Task<List<RecommendationReadDto>> GetRecommendationsForUserAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            // 1) Get the set of genre IDs the user prefers
            var preferredGenres = _context.UserGenres
                .AsNoTracking()
                .Where(ug => ug.UserId == userId)
                .Select(ug => ug.GenreId);

            // 2) Query books in those genres
            var query = _context.Books
                .AsNoTracking()
                .Where(b => preferredGenres.Contains(b.GenreId))
                .Include(b => b.Genre)
                .Include(b => b.Author)
                .OrderBy(b => b.BookId)
                .Select(b => new RecommendationReadDto
                {
                    BookId   = b.BookId,
                    Title    = b.Title,
                    Genre    = b.Genre.Name,
                    Author   = b.Author.Name,
                    CoverUrl = b.CoverUrl ?? string.Empty
                })
                .Take(10);

            return await query.ToListAsync(cancellationToken);
        }
    }
}