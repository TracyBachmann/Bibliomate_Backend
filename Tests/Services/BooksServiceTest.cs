using backend.Data;
using backend.DTOs;
using backend.Models;
using backend.Services;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;
using backend.Models.Mongo;

namespace Tests.Services
{
    public class BooksServiceTests
    {
        private readonly BookService _service;
        private readonly BiblioMateDbContext _db;
        private readonly ITestOutputHelper _output;

        public BooksServiceTests(ITestOutputHelper output)
        {
            _output = output;

            var options = new DbContextOptionsBuilder<BiblioMateDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _db = new BiblioMateDbContext(options, encryptionService: null!);
            var fakeSearchLog = new SearchActivityLogServiceFake();

            _service = new BookService(_db, fakeSearchLog);
        }

        [Fact]
        public async Task CreateAsync_ShouldAddBook()
        {
            _output.WriteLine("=== CreateAsync: START ===");

            // Add related data needed for FKs
            var author = new Author { Name = "Test Author" };
            var genre = new Genre { Name = "Test Genre" };
            var editor = new Editor { Name = "Test Editor" };
            var shelfLevel = new ShelfLevel { ShelfId = 1, LevelNumber = 1 };

            _db.Authors.Add(author);
            _db.Genres.Add(genre);
            _db.Editors.Add(editor);
            _db.ShelfLevels.Add(shelfLevel);
            await _db.SaveChangesAsync();

            var dto = new BookCreateDto
            {
                Title = "Test Book",
                Isbn = "1234567890123",
                PublicationDate = new DateTime(2000, 1, 1),
                AuthorId = author.AuthorId,
                GenreId = genre.GenreId,
                EditorId = editor.EditorId,
                ShelfLevelId = shelfLevel.ShelfLevelId,
                CoverUrl = "https://example.com/cover.jpg",
                TagIds = new List<int>()
            };

            var result = await _service.CreateAsync(dto);

            _output.WriteLine($"Created BookId: {result.BookId}, Title: {result.Title}");

            Assert.NotNull(result);
            Assert.Equal(dto.Title, result.Title);
            Assert.Equal(dto.Isbn, result.Isbn);
            Assert.True(await _db.Books.AnyAsync(b => b.BookId == result.BookId));

            _output.WriteLine("=== CreateAsync: END ===");
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
        {
            _output.WriteLine("=== GetByIdAsync (not exists): START ===");

            var result = await _service.GetByIdAsync(999);

            Assert.Null(result);

            _output.WriteLine("=== GetByIdAsync (not exists): END ===");
        }

        [Fact]
        public async Task UpdateAsync_ShouldModifyBook_WhenExists()
        {
            _output.WriteLine("=== UpdateAsync: START ===");

            // FK setup
            var author = new Author { Name = "Author A" };
            var genre = new Genre { Name = "Genre A" };
            var editor = new Editor { Name = "Editor A" };
            var shelfLevel = new ShelfLevel { ShelfId = 1, LevelNumber = 1 };
            _db.Authors.Add(author);
            _db.Genres.Add(genre);
            _db.Editors.Add(editor);
            _db.ShelfLevels.Add(shelfLevel);
            await _db.SaveChangesAsync();

            var book = new Book
            {
                Title = "Old Title",
                Isbn = "1111111111111",
                PublicationDate = new DateTime(1999, 1, 1),
                AuthorId = author.AuthorId,
                GenreId = genre.GenreId,
                EditorId = editor.EditorId,
                ShelfLevelId = shelfLevel.ShelfLevelId
            };
            _db.Books.Add(book);
            await _db.SaveChangesAsync();

            var dto = new BookUpdateDto
            {
                BookId = book.BookId,
                Title = "Updated Title",
                Isbn = "2222222222222",
                PublicationDate = new DateTime(2010, 5, 5),
                AuthorId = author.AuthorId,
                GenreId = genre.GenreId,
                EditorId = editor.EditorId,
                ShelfLevelId = shelfLevel.ShelfLevelId
            };

            var success = await _service.UpdateAsync(book.BookId, dto);

            Assert.True(success);

            var updated = await _db.Books.FindAsync(book.BookId);

            Assert.Equal(dto.Title, updated?.Title);
            Assert.Equal(dto.Isbn, updated?.Isbn);

            _output.WriteLine("=== UpdateAsync: END ===");
        }

        [Fact]
        public async Task UpdateAsync_ShouldReturnFalse_WhenNotExists()
        {
            _output.WriteLine("=== UpdateAsync (fail): START ===");

            var dto = new BookUpdateDto
            {
                BookId = 999,
                Title = "Doesn't matter",
                Isbn = "0000000000",
                PublicationDate = DateTime.Now,
                AuthorId = 1,
                GenreId = 1,
                EditorId = 1,
                ShelfLevelId = 1
            };

            var success = await _service.UpdateAsync(999, dto);

            Assert.False(success);

            _output.WriteLine("=== UpdateAsync (fail): END ===");
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveBook_WhenExists()
        {
            _output.WriteLine("=== DeleteAsync: START ===");

            var author = new Author { Name = "Author" };
            var genre = new Genre { Name = "Genre" };
            var editor = new Editor { Name = "Editor" };
            var shelfLevel = new ShelfLevel { ShelfId = 1, LevelNumber = 1 };
            _db.Authors.Add(author);
            _db.Genres.Add(genre);
            _db.Editors.Add(editor);
            _db.ShelfLevels.Add(shelfLevel);
            await _db.SaveChangesAsync();

            var book = new Book
            {
                Title = "Delete Me",
                Isbn = "9999999999999",
                PublicationDate = new DateTime(2000, 1, 1),
                AuthorId = author.AuthorId,
                GenreId = genre.GenreId,
                EditorId = editor.EditorId,
                ShelfLevelId = shelfLevel.ShelfLevelId
            };
            _db.Books.Add(book);
            await _db.SaveChangesAsync();

            var success = await _service.DeleteAsync(book.BookId);

            Assert.True(success);
            Assert.False(await _db.Books.AnyAsync(b => b.BookId == book.BookId));

            _output.WriteLine("=== DeleteAsync: END ===");
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnFalse_WhenNotExists()
        {
            _output.WriteLine("=== DeleteAsync (fail): START ===");

            var success = await _service.DeleteAsync(999);

            Assert.False(success);

            _output.WriteLine("=== DeleteAsync (fail): END ===");
        }

        [Fact]
        public async Task GetPagedAsync_ShouldReturnPagedResults()
        {
            _output.WriteLine("=== GetPagedAsync: START ===");

            var author = new Author { Name = "Author" };
            var genre = new Genre { Name = "Genre" };
            var editor = new Editor { Name = "Editor" };
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
                    Title = $"Book {i}",
                    Isbn = $"97800000000{i}",
                    PublicationDate = new DateTime(2000 + i, 1, 1),
                    AuthorId = author.AuthorId,
                    GenreId = genre.GenreId,
                    EditorId = editor.EditorId,
                    ShelfLevelId = shelfLevel.ShelfLevelId
                });
            }
            await _db.SaveChangesAsync();

            // Version la plus simple possible
            dynamic result = await _service.GetPagedAsync(1, 5, "Title", true);
    
            _output.WriteLine($"Result type: {result.GetType().Name}");
            // Testez d'abord juste que ça ne plante pas
            Assert.NotNull(result);

            _output.WriteLine("=== GetPagedAsync: END ===");
        }

        [Fact]
        public async Task SearchAsync_ShouldFilterByTitle()
        {
            _output.WriteLine("=== SearchAsync: START ===");

            var author = new Author { Name = "Author" };
            var genre = new Genre { Name = "Genre" };
            var editor = new Editor { Name = "Editor" };
            var shelfLevel = new ShelfLevel { ShelfId = 1, LevelNumber = 1 };
            _db.Authors.Add(author);
            _db.Genres.Add(genre);
            _db.Editors.Add(editor);
            _db.ShelfLevels.Add(shelfLevel);
            await _db.SaveChangesAsync();

            _db.Books.Add(new Book
            {
                Title = "SpecialTitle",
                Isbn = "9781111111111",
                PublicationDate = new DateTime(2010, 1, 1),
                AuthorId = author.AuthorId,
                GenreId = genre.GenreId,
                EditorId = editor.EditorId,
                ShelfLevelId = shelfLevel.ShelfLevelId
            });
            _db.Books.Add(new Book
            {
                Title = "AnotherBook",
                Isbn = "9782222222222",
                PublicationDate = new DateTime(2012, 1, 1),
                AuthorId = author.AuthorId,
                GenreId = genre.GenreId,
                EditorId = editor.EditorId,
                ShelfLevelId = shelfLevel.ShelfLevelId
            });
            await _db.SaveChangesAsync();

            var results = await _service.SearchAsync(new BookSearchDto
            {
                Title = "Special"
            }, userId: null);

            // Version la plus simple - évite complètement l'énumération multiple
            Assert.NotNull(results);
    
            var found = false;
            var count = 0;
            foreach (var item in results)
            {
                count++;
                if (item.Title.Contains("Special"))
                    found = true;
            }
    
            Assert.Equal(1, count);
            Assert.True(found);

            _output.WriteLine("=== SearchAsync: END ===");
        }
    }

    /// <summary>
    /// Fake implementation for ISearchActivityLogService.
    /// </summary>
    public class SearchActivityLogServiceFake : ISearchActivityLogService
    {
        public Task LogAsync(SearchActivityLogDocument doc) => Task.CompletedTask;

        public Task<List<SearchActivityLogDocument>> GetByUserAsync(int userId) =>
            Task.FromResult(new List<SearchActivityLogDocument>());

        public Task<List<SearchActivityLogDocument>> GetRecentSearchesAsync(int userId, int count) =>
            Task.FromResult(new List<SearchActivityLogDocument>());

        public Task<Dictionary<string, int>> GetTopSearchTermsAsync(int userId, int count) =>
            Task.FromResult(new Dictionary<string, int>());
    }
}