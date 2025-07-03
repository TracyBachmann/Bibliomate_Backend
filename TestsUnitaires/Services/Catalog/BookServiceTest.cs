using System.Text;
using BackendBiblioMate.Data;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Interfaces;
using BackendBiblioMate.Models;
using BackendBiblioMate.Models.Mongo;
using BackendBiblioMate.Services.Catalog;
using BackendBiblioMate.Services.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit.Abstractions;

namespace TestsUnitaires.Services.Catalog
{
    /// <summary>
    /// Unit tests for <see cref="BookService"/>.
    /// Verifies CRUD, pagination, and search logic using an in-memory EF Core provider.
    /// </summary>
    public class BookServiceTest
    {
        private readonly BookService _service;
        private readonly BiblioMateDbContext _db;
        private readonly ITestOutputHelper _output;

        /// <summary>
        /// Initializes the test context with an in-memory EF Core database and encryption.
        /// </summary>
        public BookServiceTest(ITestOutputHelper output)
        {
            _output = output;

            // 1) Build in-memory EF options
            var options = new DbContextOptionsBuilder<BiblioMateDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            // 2) Provide a 32-byte Base64 key for EncryptionService
            var base64Key = Convert.ToBase64String(
                Encoding.UTF8.GetBytes("12345678901234567890123456789012")
            );
            IConfiguration config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Encryption:Key"] = base64Key
                })
                .Build();
            var encryptionService = new EncryptionService(config);

            // 3) Instantiate DbContext with EncryptionService
            _db = new BiblioMateDbContext(options, encryptionService);

            // 4) Fake search-log service and BookService under test
            var fakeSearchLog = new FakeSearchActivityLogService();
            _service = new BookService(_db, fakeSearchLog);
        }

        /// <summary>
        /// Ensures that CreateAsync adds a new book.
        /// </summary>
        [Fact]
        public async Task CreateAsync_ShouldAddBook()
        {
            // Arrange
            var author     = new Author { Name = "Test Author" };
            var genre      = new Genre  { Name = "Test Genre"  };
            var editor     = new Editor { Name = "Test Editor" };
            var shelfLevel = new ShelfLevel { ShelfId = 1, LevelNumber = 1 };
            _db.Authors.Add(author);
            _db.Genres.Add(genre);
            _db.Editors.Add(editor);
            _db.ShelfLevels.Add(shelfLevel);
            await _db.SaveChangesAsync();

            var dto = new BookCreateDto
            {
                Title           = "Test Book",
                Isbn            = "1234567890123",
                PublicationDate = new DateTime(2000, 1, 1),
                AuthorId        = author.AuthorId,
                GenreId         = genre.GenreId,
                EditorId        = editor.EditorId,
                ShelfLevelId    = shelfLevel.ShelfLevelId,
                CoverUrl        = "https://example.com/cover.jpg",
                TagIds          = new List<int>()
            };

            // Act
            var result = await _service.CreateAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(dto.Title, result.Title);
            Assert.Equal(dto.Isbn, result.Isbn);
            Assert.True(await _db.Books.AnyAsync(b => b.BookId == result.BookId));
        }

        /// <summary>
        /// Ensures that GetByIdAsync returns null when the book does not exist.
        /// </summary>
        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
        {
            // Act
            var result = await _service.GetByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Ensures that UpdateAsync modifies an existing book.
        /// </summary>
        [Fact]
        public async Task UpdateAsync_ShouldModifyBook_WhenExists()
        {
            // Arrange
            var author     = new Author { Name = "Author A" };
            var genre      = new Genre  { Name = "Genre A"  };
            var editor     = new Editor { Name = "Editor A" };
            var shelfLevel = new ShelfLevel { ShelfId = 1, LevelNumber = 1 };
            _db.Authors.Add(author);
            _db.Genres.Add(genre);
            _db.Editors.Add(editor);
            _db.ShelfLevels.Add(shelfLevel);
            await _db.SaveChangesAsync();

            var book = new Book
            {
                Title           = "Old Title",
                Isbn            = "1111111111111",
                PublicationDate = new DateTime(1999, 1, 1),
                AuthorId        = author.AuthorId,
                GenreId         = genre.GenreId,
                EditorId        = editor.EditorId,
                ShelfLevelId    = shelfLevel.ShelfLevelId
            };
            _db.Books.Add(book);
            await _db.SaveChangesAsync();

            var dto = new BookUpdateDto
            {
                BookId          = book.BookId,
                Title           = "Updated Title",
                Isbn            = "2222222222222",
                PublicationDate = new DateTime(2010, 5, 5),
                AuthorId        = author.AuthorId,
                GenreId         = genre.GenreId,
                EditorId        = editor.EditorId,
                ShelfLevelId    = shelfLevel.ShelfLevelId
            };

            // Act
            var success = await _service.UpdateAsync(book.BookId, dto);

            // Assert
            Assert.True(success);
            var updated = await _db.Books.FindAsync(book.BookId);
            Assert.Equal(dto.Title, updated?.Title);
            Assert.Equal(dto.Isbn,  updated?.Isbn);
        }

        /// <summary>
        /// Ensures that DeleteAsync removes an existing book.
        /// </summary>
        [Fact]
        public async Task DeleteAsync_ShouldRemoveBook_WhenExists()
        {
            // Arrange
            var author     = new Author { Name = "Author" };
            var genre      = new Genre  { Name = "Genre"  };
            var editor     = new Editor { Name = "Editor" };
            var shelfLevel = new ShelfLevel { ShelfId = 1, LevelNumber = 1 };
            _db.Authors.Add(author);
            _db.Genres.Add(genre);
            _db.Editors.Add(editor);
            _db.ShelfLevels.Add(shelfLevel);
            await _db.SaveChangesAsync();

            var book = new Book
            {
                Title           = "Delete Me",
                Isbn            = "9999999999999",
                PublicationDate = new DateTime(2000, 1, 1),
                AuthorId        = author.AuthorId,
                GenreId         = genre.GenreId,
                EditorId        = editor.EditorId,
                ShelfLevelId    = shelfLevel.ShelfLevelId
            };
            _db.Books.Add(book);
            await _db.SaveChangesAsync();

            // Act
            var removed = await _service.DeleteAsync(book.BookId);

            // Assert
            Assert.True(removed);
            Assert.False(await _db.Books.AnyAsync(b => b.BookId == book.BookId));
        }

        /// <summary>
        /// Ensures that GetPagedAsync returns paginated results.
        /// </summary>
        [Fact]
        public async Task GetPagedAsync_ShouldReturnPagedResults()
        {
            // Arrange
            var author     = new Author { Name = "Author" };
            var genre      = new Genre  { Name = "Genre"  };
            var editor     = new Editor { Name = "Editor" };
            var shelfLevel = new ShelfLevel { ShelfId = 1, LevelNumber = 1 };
            _db.Authors.Add(author);
            _db.Genres.Add(genre);
            _db.Editors.Add(editor);
            _db.ShelfLevels.Add(shelfLevel);
            await _db.SaveChangesAsync();

            for (int i = 1; i <= 10; i++)
            {
                _db.Books.Add(new Book
                {
                    Title           = $"Book {i}",
                    Isbn            = $"97800000000{i}",
                    PublicationDate = new DateTime(2000 + i, 1, 1),
                    AuthorId        = author.AuthorId,
                    GenreId         = genre.GenreId,
                    EditorId        = editor.EditorId,
                    ShelfLevelId    = shelfLevel.ShelfLevelId
                });
            }
            await _db.SaveChangesAsync();

            // Act
            var (page, eTag, notModified) = await _service.GetPagedAsync(
                pageNumber: 1,
                pageSize:   5,
                sortBy:     "Title",
                ascending:  true
            );

            // Assert
            Assert.NotNull(page);
            Assert.Equal(5, page.Items.Count());
            Assert.False(string.IsNullOrWhiteSpace(eTag));
            Assert.Null(notModified);
        }

        /// <summary>
        /// Ensures that SearchAsync filters results by title.
        /// </summary>
        [Fact]
        public async Task SearchAsync_ShouldFilterByTitle()
        {
            // Arrange
            var author     = new Author { Name = "Author" };
            var genre      = new Genre  { Name = "Genre"  };
            var editor     = new Editor { Name = "Editor" };
            var shelfLevel = new ShelfLevel { ShelfId = 1, LevelNumber = 1 };
            _db.Authors.Add(author);
            _db.Genres.Add(genre);
            _db.Editors.Add(editor);
            _db.ShelfLevels.Add(shelfLevel);
            await _db.SaveChangesAsync();

            _db.Books.AddRange(new[]
            {
                new Book
                {
                    Title           = "SpecialTitle",
                    Isbn            = "9781111111111",
                    PublicationDate = new DateTime(2010, 1, 1),
                    AuthorId        = author.AuthorId,
                    GenreId         = genre.GenreId,
                    EditorId        = editor.EditorId,
                    ShelfLevelId    = shelfLevel.ShelfLevelId
                },
                new Book
                {
                    Title           = "AnotherBook",
                    Isbn            = "9782222222222",
                    PublicationDate = new DateTime(2012, 1, 1),
                    AuthorId        = author.AuthorId,
                    GenreId         = genre.GenreId,
                    EditorId        = editor.EditorId,
                    ShelfLevelId    = shelfLevel.ShelfLevelId
                }
            });
            await _db.SaveChangesAsync();

            // Act
            var results = await _service.SearchAsync(
                new BookSearchDto { Title = "Special" },
                userId: null
            );
            var resultsList = results.ToList();

            // Assert
            Assert.Single(resultsList);
            Assert.Contains(resultsList, b => b.Title.Contains("Special"));
        }

        /// <summary>
        /// Fake implementation of <see cref="ISearchActivityLogService"/> for testing.
        /// </summary>
        private class FakeSearchActivityLogService : ISearchActivityLogService
        {
            public Task LogAsync(SearchActivityLogDocument doc, CancellationToken cancellationToken = default)
                => Task.CompletedTask;

            public Task<List<SearchActivityLogDocument>> GetByUserAsync(int userId, CancellationToken cancellationToken = default)
                => Task.FromResult(new List<SearchActivityLogDocument>());
        }
    }
}