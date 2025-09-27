using System.ComponentModel.DataAnnotations;
using System.Text;
using BackendBiblioMate.Data;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Interfaces;
using BackendBiblioMate.Models;
using BackendBiblioMate.Services.Catalog;
using BackendBiblioMate.Services.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit.Abstractions;

namespace UnitTestsBiblioMate.Services.Catalog
{
    /// <summary>
    /// Unit tests for <see cref="BookService"/>.
    /// Verifies:
    /// <list type="bullet">
    ///   <item><description>CRUD operations (Create, Read, Update, Delete)</description></item>
    ///   <item><description>Pagination via <c>GetPagedAsync</c></description></item>
    ///   <item><description>Search filters across multiple fields</description></item>
    ///   <item><description>Handling of tags, stocks, and shelf locations</description></item>
    /// </list>
    /// Uses EF Core InMemory provider to simulate a real database.
    /// </summary>
    public class BookServiceTest : IDisposable
    {
        private readonly BiblioMateDbContext _db;
        private readonly BookService _service;
        private readonly ITestOutputHelper _output;

        /// <summary>
        /// Initializes a new in-memory DbContext and service instance.
        /// </summary>
        public BookServiceTest(ITestOutputHelper output)
        {
            _output = output;

            var options = new DbContextOptionsBuilder<BiblioMateDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var base64Key = Convert.ToBase64String(Encoding.UTF8.GetBytes("12345678901234567890123456789012"));
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?> { ["Encryption:Key"] = base64Key })
                .Build();

            var encryption = new EncryptionService(config);
            _db = new BiblioMateDbContext(options, encryption);

            // searchLog is optional -> null in this test setup
            _service = new BookService(_db, searchLog: null);
        }

        /// <summary>
        /// Disposes the in-memory DbContext after each test.
        /// </summary>
        public void Dispose() => _db.Dispose();

        // ----------- helpers -----------

        /// <summary>
        /// Seeds the database with a minimal catalog hierarchy:
        /// Author, Genre, Editor, Zone, Shelf, and ShelfLevel.
        /// </summary>
        private (Author author, Genre genre, Editor editor, Zone zone, Shelf shelf, ShelfLevel level) SeedCatalogLocation()
        {
            var author = _db.Authors.Add(new Author { Name = "A1" }).Entity;
            var genre  = _db.Genres.Add(new Genre  { Name = "G1" }).Entity;
            var editor = _db.Editors.Add(new Editor { Name = "E1" }).Entity;

            var zone   = _db.Zones.Add(new Zone { Name = "Z1", FloorNumber = 2, AisleCode = "A-12" }).Entity;
            var shelf  = _db.Shelves.Add(new Shelf { ZoneId = zone.ZoneId, Name = "R-01" }).Entity;
            var level  = _db.ShelfLevels.Add(new ShelfLevel { ShelfId = shelf.ShelfId, LevelNumber = 3 }).Entity;

            _db.SaveChanges();
            return (author, genre, editor, zone, shelf, level);
        }

        // ----------- Create -----------

        /// <summary>
        /// CreateAsync should add a new book with a shelf level ID
        /// and return a DTO with projected catalog/location info.
        /// </summary>
        [Fact]
        public async Task CreateAsync_WithShelfLevelId_AddsBook_MapsProjection()
        {
            var (author, genre, editor, zone, shelf, level) = SeedCatalogLocation();

            var dto = new BookCreateDto
            {
                Title           = "Test Book",
                Isbn            = "9780000000001",
                Description     = "Desc",
                PublicationDate = new DateTime(2015, 6, 1),
                AuthorId        = author.AuthorId,
                GenreId         = genre.GenreId,
                EditorId        = editor.EditorId,
                ShelfLevelId    = level.ShelfLevelId,
                CoverUrl        = "cover.png",
                TagIds          = new List<int>(),
                StockQuantity   = 2
            };

            var result = await _service.CreateAsync(dto);

            Assert.NotNull(result);
            Assert.True(result.BookId > 0);
            Assert.Equal("Test Book", result.Title);
            Assert.Equal("9780000000001", result.Isbn);
            Assert.Equal(2015, result.PublicationYear);
            Assert.Equal("A1", result.AuthorName);
            Assert.Equal("G1", result.GenreName);
            Assert.Equal("E1", result.EditorName);
            Assert.Equal(2, result.StockQuantity);
            Assert.Equal(2, result.Floor);
            Assert.Equal("A-12", result.Aisle);
            Assert.Equal("R-01", result.Rayon);
            Assert.Equal(3, result.Shelf);
        }

        /// <summary>
        /// CreateAsync without ShelfLevelId or Location should throw a <see cref="ValidationException"/>.
        /// </summary>
        [Fact]
        public async Task CreateAsync_WithoutShelfLevelIdAndWithoutLocation_ThrowsValidationException()
        {
            var (author, genre, editor, _, _, _) = SeedCatalogLocation();

            var dto = new BookCreateDto
            {
                Title           = "X",
                Isbn            = "9780000000002",
                PublicationDate = new DateTime(2020, 1, 1),
                AuthorId        = author.AuthorId,
                GenreId         = genre.GenreId,
                EditorId        = editor.EditorId
                // Missing ShelfLevelId and Location -> should trigger ValidationException
            };

            await Assert.ThrowsAsync<ValidationException>(() => _service.CreateAsync(dto));
        }

        /// <summary>
        /// CreateAsync with a location DTO should call the ILocationService
        /// to resolve or create the corresponding ShelfLevel.
        /// </summary>
        [Fact]
        public async Task CreateAsync_WithLocation_UsesLocationServiceToResolveShelfLevel()
        {
            var (author, genre, editor, zone, shelf, level) = SeedCatalogLocation();

            var locDto = new LocationEnsureDto
            {
                FloorNumber = zone.FloorNumber,
                AisleCode   = zone.AisleCode,
                ShelfName   = shelf.Name,
                LevelNumber = level.LevelNumber
            };

            var locationSvc = new Mock<ILocationService>();
            locationSvc
                .Setup(s => s.EnsureAsync(It.IsAny<LocationEnsureDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new LocationReadDto
                {
                    ZoneId       = zone.ZoneId,
                    ShelfId      = shelf.ShelfId,
                    ShelfLevelId = level.ShelfLevelId,
                    FloorNumber  = zone.FloorNumber,
                    AisleCode    = zone.AisleCode,
                    ShelfName    = shelf.Name,
                    LevelNumber  = level.LevelNumber
                });

            var svc = new BookService(_db, searchLog: null, location: locationSvc.Object);

            var dto = new BookCreateDto
            {
                Title           = "Loc Book",
                Isbn            = "9780000000003",
                PublicationDate = new DateTime(2021, 1, 1),
                AuthorId        = author.AuthorId,
                GenreId         = genre.GenreId,
                EditorId        = editor.EditorId,
                Location        = locDto
            };

            var res = await svc.CreateAsync(dto);

            Assert.NotNull(res);
            Assert.Equal("Loc Book", res.Title);

            locationSvc.Verify(s => s.EnsureAsync(
                    It.Is<LocationEnsureDto>(l =>
                        l.FloorNumber == locDto.FloorNumber &&
                        l.AisleCode   == locDto.AisleCode &&
                        l.ShelfName   == locDto.ShelfName &&
                        l.LevelNumber == locDto.LevelNumber),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        // ----------- GetById -----------

        /// <summary>
        /// GetByIdAsync should return null when the book does not exist.
        /// </summary>
        [Fact]
        public async Task GetByIdAsync_WhenNotExists_ReturnsNull()
        {
            var result = await _service.GetByIdAsync(999);
            Assert.Null(result);
        }

        /// <summary>
        /// GetByIdAsync should return a fully projected DTO when the book exists.
        /// </summary>
        [Fact]
        public async Task GetByIdAsync_WhenExists_ReturnsProjectedDto()
        {
            var (author, genre, editor, zone, shelf, level) = SeedCatalogLocation();

            var b = _db.Books.Add(new Book
            {
                Title           = "B1",
                Isbn            = "9780000000100",
                PublicationDate = new DateTime(2010, 2, 2),
                AuthorId        = author.AuthorId,
                GenreId         = genre.GenreId,
                EditorId        = editor.EditorId,
                ShelfLevelId    = level.ShelfLevelId
            }).Entity;

            _db.Stocks.Add(new Stock { BookId = b.BookId, Quantity = 1 });
            await _db.SaveChangesAsync();

            var dto = await _service.GetByIdAsync(b.BookId);
            Assert.NotNull(dto);
            Assert.Equal("B1", dto!.Title);
            Assert.Equal(2010, dto.PublicationYear);
            Assert.Equal("A1", dto.AuthorName);
            Assert.Equal("G1", dto.GenreName);
            Assert.Equal("E1", dto.EditorName);
        }

        // ----------- Update -----------

        /// <summary>
        /// UpdateAsync should modify fields, update tags, and adjust stock quantity.
        /// </summary>
        [Fact]
        public async Task UpdateAsync_ModifiesFields_AndUpdatesTagsAndStock_WhenProvided()
        {
            var (author, genre, editor, zone, shelf, level) = SeedCatalogLocation();

            var tag1 = _db.Tags.Add(new Tag { Name = "T1" }).Entity;
            var tag2 = _db.Tags.Add(new Tag { Name = "T2" }).Entity;
            await _db.SaveChangesAsync();

            var book = _db.Books.Add(new Book
            {
                Title           = "Old",
                Isbn            = "111",
                Description     = "D",
                PublicationDate = new DateTime(2000, 1, 1),
                AuthorId        = author.AuthorId,
                GenreId         = genre.GenreId,
                EditorId        = editor.EditorId,
                ShelfLevelId    = level.ShelfLevelId
            }).Entity;
            _db.Stocks.Add(new Stock { BookId = book.BookId, Quantity = 1 });
            await _db.SaveChangesAsync();

            var dto = new BookUpdateDto
            {
                BookId          = book.BookId,
                Title           = "New",
                Isbn            = "222",
                Description     = "ND",
                PublicationDate = new DateTime(2012, 5, 5),
                AuthorId        = author.AuthorId,
                GenreId         = genre.GenreId,
                EditorId        = editor.EditorId,
                ShelfLevelId    = level.ShelfLevelId,
                TagIds          = new List<int> { tag1.TagId, tag2.TagId },
                StockQuantity   = 5
            };

            var ok = await _service.UpdateAsync(book.BookId, dto);

            Assert.True(ok);

            var updated = await _db.Books.FindAsync(book.BookId);
            Assert.Equal("New", updated!.Title);
            Assert.Equal("222", updated.Isbn);

            var bt = await _db.BookTags.Where(x => x.BookId == book.BookId).Select(x => x.TagId).ToListAsync();
            Assert.True(bt.Contains(tag1.TagId) && bt.Contains(tag2.TagId));

            var stock = await _db.Stocks.FirstAsync(s => s.BookId == book.BookId);
            Assert.Equal(5, stock.Quantity);
        }

        /// <summary>
        /// UpdateAsync should return false when the book does not exist.
        /// </summary>
        [Fact]
        public async Task UpdateAsync_ReturnsFalse_WhenBookMissing()
        {
            var dto = new BookUpdateDto { BookId = 123, Title = "X", PublicationDate = DateTime.Today };
            var ok = await _service.UpdateAsync(123, dto);
            Assert.False(ok);
        }

        // ----------- Delete -----------

        /// <summary>
        /// DeleteAsync should remove a book, its tags, and its stock entries.
        /// </summary>
        [Fact]
        public async Task DeleteAsync_RemovesBook_Tags_AndStock()
        {
            var (author, genre, editor, zone, shelf, level) = SeedCatalogLocation();
            var tag = _db.Tags.Add(new Tag { Name = "T" }).Entity;
            await _db.SaveChangesAsync();

            var b = _db.Books.Add(new Book
            {
                Title           = "ToDel",
                Isbn            = "X",
                PublicationDate = new DateTime(2011, 1, 1),
                AuthorId        = author.AuthorId,
                GenreId         = genre.GenreId,
                EditorId        = editor.EditorId,
                ShelfLevelId    = level.ShelfLevelId
            }).Entity;
            _db.BookTags.Add(new BookTag { BookId = b.BookId, TagId = tag.TagId });
            _db.Stocks.Add(new Stock { BookId = b.BookId, Quantity = 2 });
            await _db.SaveChangesAsync();

            var ok = await _service.DeleteAsync(b.BookId);
            Assert.True(ok);

            Assert.False(await _db.Books.AnyAsync(x => x.BookId == b.BookId));
            Assert.False(await _db.BookTags.AnyAsync(x => x.BookId == b.BookId));
            Assert.False(await _db.Stocks.AnyAsync(x => x.BookId == b.BookId));
        }

        /// <summary>
        /// DeleteAsync should return false when the book does not exist.
        /// </summary>
        [Fact]
        public async Task DeleteAsync_ReturnsFalse_WhenBookMissing()
        {
            var ok = await _service.DeleteAsync(999);
            Assert.False(ok);
        }

        // ----------- Paging -----------

        /// <summary>
        /// GetPagedAsync should return the correct slice of books,
        /// with null ETag values when caching is not used.
        /// NOTE: Tri sur "title" => lexicographique ("Book 1", "Book 10", "Book 2", "Book 3", ...)
        /// Page 2 / size 3 => éléments 4-6 dans cet ordre => "Book 3", "Book 4", "Book 5".
        /// </summary>
        [Fact]
        public async Task GetPagedAsync_ReturnsCorrectPage_AndNullEtags()
        {
            var (author, genre, editor, zone, shelf, level) = SeedCatalogLocation();

            for (int i = 1; i <= 10; i++)
            {
                _db.Books.Add(new Book
                {
                    Title           = $"Book {i}",
                    Isbn            = $"9780000000{i:D3}",
                    PublicationDate = new DateTime(2000 + i, 1, 1),
                    AuthorId        = author.AuthorId,
                    GenreId         = genre.GenreId,
                    EditorId        = editor.EditorId,
                    ShelfLevelId    = level.ShelfLevelId
                });
            }
            await _db.SaveChangesAsync();

            var (page, eTag, notModified) = await _service.GetPagedAsync(2, 3, sortBy: "title", ascending: true);
            Assert.NotNull(page);
            Assert.Equal(2, page.PageNumber);
            Assert.Equal(3, page.PageSize);
            Assert.Equal(10, page.TotalCount);
            Assert.Equal(4, page.TotalPages);
            Assert.Equal(3, page.Items.Count());

            var titles = page.Items.Select(i => i.Title).ToList();
            // Correction: tri lexicographique -> "Book 3","Book 4","Book 5"
            Assert.Equal(new[] { "Book 3", "Book 4", "Book 5" }, titles);

            Assert.Null(eTag);
            Assert.Null(notModified);
        }

        // ----------- Search -----------

        /// <summary>
        /// SearchAsync should apply filters across title, author, genre, editor, ISBN,
        /// publication year range, tags, description, and exclusion terms.
        /// </summary>
        [Fact]
        public async Task SearchAsync_FiltersByTitle_Author_Genre_Publisher_Isbn_Range_Tags_Description_Exclude()
        {
            var (author, genre, editor, zone, shelf, level) = SeedCatalogLocation();
            var tagFoo = _db.Tags.Add(new Tag { Name = "Foo" }).Entity;
            var tagBar = _db.Tags.Add(new Tag { Name = "Bar" }).Entity;
            await _db.SaveChangesAsync();

            var b1 = _db.Books.Add(new Book
            {
                Title           = "C# in Depth",
                Isbn            = "9781617294532",
                Description     = "Deep dive",
                PublicationDate = new DateTime(2019, 1, 1),
                AuthorId        = author.AuthorId,
                GenreId         = genre.GenreId,
                EditorId        = editor.EditorId,
                ShelfLevelId    = level.ShelfLevelId
            }).Entity;
            var b2 = _db.Books.Add(new Book
            {
                Title           = "Clean Code",
                Isbn            = "9780132350884",
                Description     = "A handbook of agile software craftsmanship",
                PublicationDate = new DateTime(2008, 1, 1),
                AuthorId        = author.AuthorId,
                GenreId         = genre.GenreId,
                EditorId        = editor.EditorId,
                ShelfLevelId    = level.ShelfLevelId
            }).Entity;

            _db.BookTags.AddRange(
                new BookTag { BookId = b1.BookId, TagId = tagFoo.TagId },
                new BookTag { BookId = b2.BookId, TagId = tagBar.TagId }
            );

            await _db.SaveChangesAsync();

            var dto = new BookSearchDto
            {
                Title       = "Clean",
                Author      = "A1",
                Genre       = "G1",
                Publisher   = "E1",
                Isbn        = "9780132350884",
                YearMin     = 2000,
                YearMax     = 2020,
                TagIds      = new List<int> { tagBar.TagId },
                TagNames    = new List<string> { "Bar" },
                Description = "handbook",
                Exclude     = "Deep"
            };

            var res = (await _service.SearchAsync(dto, userId: null)).ToList();

            Assert.Single(res);
            Assert.Equal("Clean Code", res[0].Title);
        }

        // ----------- Genres -----------

        /// <summary>
        /// GetAllGenresAsync should return all genres sorted alphabetically.
        /// </summary>
        [Fact]
        public async Task GetAllGenresAsync_ReturnsAlphabetical()
        {
            _db.Genres.AddRange(
                new Genre { Name = "Sci-Fi" },
                new Genre { Name = "Fantasy" },
                new Genre { Name = "Programming" }
            );
            await _db.SaveChangesAsync();

            var genres = await _service.GetAllGenresAsync();
            Assert.Equal(new[] { "Fantasy", "Programming", "Sci-Fi" }, genres);
        }
    }
}
