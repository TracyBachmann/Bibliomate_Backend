using System.Text;
using BackendBiblioMate.Data;
using BackendBiblioMate.Models;
using BackendBiblioMate.Services.Recommendations;
using BackendBiblioMate.Services.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit.Abstractions;

namespace UnitTestsBiblioMate.Services.Recommendations
{
    /// <summary>
    /// Unit tests for <see cref="RecommendationService"/>.
    /// Verifies that recommendations are filtered by user preferences,
    /// correctly mapped, and limited to 10 items.
    /// </summary>
    public class RecommendationServiceTest
    {
        private readonly RecommendationService _service;
        private readonly BiblioMateDbContext   _db;
        private readonly ITestOutputHelper     _output;

        public RecommendationServiceTest(ITestOutputHelper output)
        {
            _output = output;

            // In-memory EF Core database with encryption
            var options = new DbContextOptionsBuilder<BiblioMateDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

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
        /// When the user has genre preferences,
        /// only books in those genres should be returned,
        /// with null CoverUrl mapped to empty string.
        /// </summary>
        [Fact]
        public async Task GetRecommendationsForUserAsync_ShouldReturnMatchingBooks()
        {
            _output.WriteLine("=== GetRecommendationsForUserAsync (matching) ===");

            // Seed genres
            var fantasy = new Genre { Name = "Fantasy" };
            var scifi   = new Genre { Name = "Sci-Fi" };
            _db.Genres.AddRange(fantasy, scifi);
            await _db.SaveChangesAsync();

            // User 1 prefers Fantasy
            const int userId = 1;
            _db.Users.Add(new User { UserId = userId, Name = "Alice" });
            _db.UserGenres.Add(new UserGenre { UserId = userId, GenreId = fantasy.GenreId });

            // Seed authors
            var tolkien = new Author { Name = "Tolkien" };
            var asimov  = new Author { Name = "Asimov" };
            _db.Authors.AddRange(tolkien, asimov);
            await _db.SaveChangesAsync();

            // Seed books: two fantasy, one sci-fi
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
                CoverUrl = null
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

            // Assert exactly the two fantasy books
            Assert.Equal(2, result.Count);
            Assert.Contains(result, r =>
                r.BookId   == 1 &&
                r.Title    == "The Hobbit" &&
                r.Genre    == "Fantasy" &&
                r.Author   == "Tolkien" &&
                r.CoverUrl == "url1"
            );
            Assert.Contains(result, r =>
                r.BookId   == 2 &&
                r.Title    == "Lord of the Rings" &&
                r.Genre    == "Fantasy" &&
                r.Author   == "Tolkien" &&
                r.CoverUrl == string.Empty
            );
        }

        /// <summary>
        /// When the user has no preferences,
        /// the result should be empty.
        /// </summary>
        [Fact]
        public async Task GetRecommendationsForUserAsync_ShouldReturnEmpty_WhenNoPreferences()
        {
            _output.WriteLine("=== GetRecommendationsForUserAsync (no prefs) ===");

            // User with no genre preferences
            const int userId = 42;
            _db.Users.Add(new User { UserId = userId, Name = "Bob" });
            await _db.SaveChangesAsync();

            // Seed one book in some other genre
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

            // Assert
            Assert.Empty(result);
        }

        /// <summary>
        /// When more than 10 matching books exist,
        /// only the first 10 (ordered by Title ascending) are returned.
        /// </summary>
        [Fact]
        public async Task GetRecommendationsForUserAsync_ShouldLimitTo10Items()
        {
            _output.WriteLine("=== GetRecommendationsForUserAsync (limit 10) ===");

            // User 5 prefers GenreX
            const int userId = 5;
            var genre = new Genre { Name = "GenreX" };
            _db.Genres.Add(genre);
            _db.Users.Add(new User { UserId = userId, Name = "Carol" });
            _db.UserGenres.Add(new UserGenre { UserId = userId, GenreId = genre.GenreId });
            await _db.SaveChangesAsync();

            // Seed author
            var author = new Author { Name = "AuthorX" };
            _db.Authors.Add(author);
            await _db.SaveChangesAsync();

            // Add 15 books all in that genre, titles Book1..Book15
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

            // Assert only 10 items, sorted by Title ascending
            Assert.Equal(10, result.Count);
            Assert.Equal("Book1",  result[0].Title);
            Assert.Equal("Book10", result[9].Title);
        }
    }
}