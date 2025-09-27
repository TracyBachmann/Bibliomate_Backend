using System.Text;
using BackendBiblioMate.Data;
using BackendBiblioMate.Models;
using BackendBiblioMate.Services.Infrastructure.Security;
using BackendBiblioMate.Services.Recommendations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit.Abstractions;

namespace UnitTestsBiblioMate.Services.Recommendations
{
    /// <summary>
    /// Unit tests for <see cref="RecommendationService"/>.
    /// Verifies that recommendations:
    /// - Match user genre preferences.
    /// - Correctly map fields (BookId, Title, Genre, Author, CoverUrl).
    /// - Are limited to a maximum of 10 results.
    /// </summary>
    public class RecommendationServiceTest
    {
        private readonly RecommendationService _service;
        private readonly BiblioMateDbContext   _db;
        private readonly ITestOutputHelper     _output;

        /// <summary>
        /// Initializes an in-memory EF Core context with EncryptionService,
        /// and creates a RecommendationService instance for testing.
        /// </summary>
        public RecommendationServiceTest(ITestOutputHelper output)
        {
            _output = output;

            // Use EF Core InMemory for isolated tests
            var options = new DbContextOptionsBuilder<BiblioMateDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            // Provide encryption service (required by DbContext)
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Encryption:Key"] = Convert.ToBase64String(
                        Encoding.UTF8.GetBytes("12345678901234567890123456789012"))
                })
                .Build();
            var encryptionService = new EncryptionService(config);

            _db      = new BiblioMateDbContext(options, encryptionService);
            _service = new RecommendationService(_db);
        }

        /// <summary>
        /// Ensures that when the user has genre preferences,
        /// books belonging to those genres are returned and mapped correctly.
        /// </summary>
        [Fact]
        public async Task GetRecommendationsForUserAsync_ShouldReturnMatchingBooks()
        {
            _output.WriteLine("=== GetRecommendationsForUserAsync (matching) ===");

            // Arrange: create genres and assign preference to user
            var fantasy = new Genre { Name = "Fantasy" };
            var scifi   = new Genre { Name = "Sci-Fi" };
            _db.Genres.AddRange(fantasy, scifi);
            await _db.SaveChangesAsync();

            const int userId = 1;
            _db.Users.Add(new User { UserId = userId, FirstName = "Alice", LastName = "Smith" });
            _db.UserGenres.Add(new UserGenre { UserId = userId, GenreId = fantasy.GenreId });

            // Authors
            var tolkien = new Author { Name = "Tolkien" };
            var asimov  = new Author { Name = "Asimov" };
            _db.Authors.AddRange(tolkien, asimov);
            await _db.SaveChangesAsync();

            // Books (two matching Fantasy, one unrelated Sci-Fi)
            _db.Books.Add(new Book
            {
                BookId   = 1,
                Title    = "The Hobbit",
                GenreId  = fantasy.GenreId,
                AuthorId = tolkien.AuthorId,
                CoverUrl = "url1"
            });
            _db.Books.Add(new Book
            {
                BookId   = 2,
                Title    = "Lord of the Rings",
                GenreId  = fantasy.GenreId,
                AuthorId = tolkien.AuthorId,
                CoverUrl = null // Should map to empty string
            });
            _db.Books.Add(new Book
            {
                BookId   = 3,
                Title    = "Foundation",
                GenreId  = scifi.GenreId,
                AuthorId = asimov.AuthorId,
                CoverUrl = "url3"
            });
            await _db.SaveChangesAsync();

            // Act
            var result = await _service.GetRecommendationsForUserAsync(userId);

            // Assert: only Fantasy books are recommended
            Assert.Equal(2, result.Count);

            // First book: has CoverUrl
            Assert.Contains(result, r =>
                r.BookId   == 1 &&
                r.Title    == "The Hobbit" &&
                r.Genre    == "Fantasy" &&
                r.Author   == "Tolkien" &&
                r.CoverUrl == "url1"
            );

            // Second book: null CoverUrl should be replaced with empty string
            Assert.Contains(result, r =>
                r.BookId   == 2 &&
                r.Title    == "Lord of the Rings" &&
                r.Genre    == "Fantasy" &&
                r.Author   == "Tolkien" &&
                r.CoverUrl == string.Empty
            );
        }

        /// <summary>
        /// Ensures that when a user has no genre preferences,
        /// the recommendation list is empty even if books exist.
        /// </summary>
        [Fact]
        public async Task GetRecommendationsForUserAsync_ShouldReturnEmpty_WhenNoPreferences()
        {
            _output.WriteLine("=== GetRecommendationsForUserAsync (no prefs) ===");

            // Arrange: user without preferences
            const int userId = 42;
            _db.Users.Add(new User { UserId = userId, FirstName = "Bob", LastName = "Johnson" });
            await _db.SaveChangesAsync();

            // Books exist, but no genres are linked to the user
            var mystery = new Genre { Name = "Mystery" };
            _db.Genres.Add(mystery);
            await _db.SaveChangesAsync();

            var agatha = new Author { Name = "Agatha" };
            _db.Authors.Add(agatha);
            await _db.SaveChangesAsync();

            _db.Books.Add(new Book
            {
                BookId   = 7,
                Title    = "Whodunit",
                GenreId  = mystery.GenreId,
                AuthorId = agatha.AuthorId,
                CoverUrl = "cover7"
            });
            await _db.SaveChangesAsync();

            // Act
            var result = await _service.GetRecommendationsForUserAsync(userId);

            // Assert: no recommendations returned
            Assert.Empty(result);
        }

        /// <summary>
        /// Ensures that recommendations are capped at 10 items maximum,
        /// even if more matching books exist in the database.
        /// </summary>
        [Fact]
        public async Task GetRecommendationsForUserAsync_ShouldLimitTo10Items()
        {
            _output.WriteLine("=== GetRecommendationsForUserAsync (limit 10) ===");

            // Arrange: create one user with one genre preference
            const int userId = 5;
            var genre = new Genre { Name = "GenreX" };
            _db.Genres.Add(genre);
            _db.Users.Add(new User { UserId = userId, FirstName = "Carol", LastName = "Lee" });
            _db.UserGenres.Add(new UserGenre { UserId = userId, GenreId = genre.GenreId });
            await _db.SaveChangesAsync();

            var author = new Author { Name = "AuthorX" };
            _db.Authors.Add(author);
            await _db.SaveChangesAsync();

            // Add 15 books in the same genre (more than 10)
            for (int i = 1; i <= 15; i++)
            {
                _db.Books.Add(new Book
                {
                    BookId   = i,
                    Title    = $"Book{i}",
                    GenreId  = genre.GenreId,
                    AuthorId = author.AuthorId,
                    CoverUrl = null
                });
            }
            await _db.SaveChangesAsync();

            // Act
            var result = await _service.GetRecommendationsForUserAsync(userId);

            // Assert: list limited to 10 items
            Assert.Equal(10, result.Count);

            // Validate ordering and mapping (first and last)
            Assert.Equal("Book1",  result[0].Title);
            Assert.Equal("Book10", result[9].Title);
        }
    }
}
