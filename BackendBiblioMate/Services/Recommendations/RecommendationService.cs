using Microsoft.EntityFrameworkCore;
using BackendBiblioMate.Data;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Interfaces;

namespace BackendBiblioMate.Services.Recommendations
{
    /// <summary>
    /// Service implementation of <see cref="IRecommendationService"/> 
    /// that generates book recommendations for users based on their preferred genres.
    /// </summary>
    public class RecommendationService : IRecommendationService
    {
        private readonly BiblioMateDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="RecommendationService"/> class.
        /// </summary>
        /// <param name="context">The EF Core database context used to access user preferences and book data.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <c>null</c>.</exception>
        public RecommendationService(BiblioMateDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Retrieves up to 10 book recommendations for the specified user, 
        /// based on the genres the user has marked as preferred.
        /// </summary>
        /// <param name="userId">The identifier of the user to recommend books for.</param>
        /// <param name="cancellationToken">Token used to observe cancellation requests.</param>
        /// <returns>
        /// A list of <see cref="RecommendationReadDto"/> objects representing recommended books.  
        /// Results are ordered by <c>BookId</c> to ensure deterministic output.
        /// </returns>
        public async Task<List<RecommendationReadDto>> GetRecommendationsForUserAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            // Step 1: Get the IDs of genres the user has marked as preferred
            var preferredGenres = _context.UserGenres
                .AsNoTracking()
                .Where(ug => ug.UserId == userId)
                .Select(ug => ug.GenreId);

            // Step 2: Query books that belong to those genres
            var query = _context.Books
                .AsNoTracking()
                .Where(b => preferredGenres.Contains(b.GenreId))
                .Include(b => b.Genre)
                .Include(b => b.Author)
                .OrderBy(b => b.BookId) // deterministic ordering
                .Select(b => new RecommendationReadDto
                {
                    BookId   = b.BookId,
                    Title    = b.Title,
                    Genre    = b.Genre.Name,
                    Author   = b.Author.Name,
                    CoverUrl = b.CoverUrl ?? string.Empty
                })
                .Take(10);

            // Step 3: Execute query and return results
            return await query.ToListAsync(cancellationToken);
        }
    }
}
