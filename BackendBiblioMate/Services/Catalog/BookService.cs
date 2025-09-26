using System.Linq.Expressions;
using BackendBiblioMate.Data;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Helpers;
using BackendBiblioMate.Interfaces;
using BackendBiblioMate.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace BackendBiblioMate.Services.Catalog
{
    /// <summary>
    /// Provides services for managing books, including CRUD operations,
    /// searching, paging, and availability management.
    /// </summary>
    public class BookService : IBookService
    {
        private readonly BiblioMateDbContext _db;
        private readonly ISearchActivityLogService? _searchLog;
        private readonly ILocationService? _location;
        private readonly IStockService? _stockSvc;

        /// <summary>
        /// Initializes a new instance of the <see cref="BookService"/> class.
        /// </summary>
        /// <param name="db">The EF Core database context.</param>
        /// <param name="searchLog">Optional service for logging search activity.</param>
        /// <param name="location">Optional service for handling book locations.</param>
        /// <param name="stockSvc">Optional service for stock management.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="db"/> is null.</exception>
        public BookService(
            BiblioMateDbContext db,
            ISearchActivityLogService? searchLog,
            ILocationService? location = null,
            IStockService? stockSvc = null)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _searchLog = searchLog;
            _location = location;
            _stockSvc = stockSvc;
        }

        #region Projection

        /// <summary>
        /// Defines the mapping from <see cref="Book"/> entities to <see cref="BookReadDto"/>.
        /// Includes availability, stock, tags, and flattened location details.
        /// </summary>
        private Expression<Func<Book, BookReadDto>> ReadProjection => b => new BookReadDto
        {
            BookId = b.BookId,
            Title = b.Title,
            Isbn = b.Isbn,
            PublicationYear = b.PublicationDate.Year,
            AuthorName = b.Author.Name,
            GenreName = b.Genre.Name,
            EditorName = b.Editor.Name,

            // Availability check: stock - active loans
            IsAvailable =
                (
                    (_db.Stocks
                        .Where(s => s.BookId == b.BookId)
                        .Select(s => (int?)s.Quantity)
                        .FirstOrDefault() ?? 0)
                    -
                    _db.Loans
                        .Where(l => l.BookId == b.BookId && l.ReturnDate == null)
                        .Select(l => (int?)l.LoanId)
                        .Count()
                ) > 0,

            // Total stock quantity
            StockQuantity = _db.Stocks
                .Where(s => s.BookId == b.BookId)
                .Select(s => (int?)s.Quantity)
                .FirstOrDefault() ?? 0,

            CoverUrl = b.CoverUrl,
            Description = b.Description,

            // Flattened location
            Floor = b.ShelfLevel.Shelf.Zone.FloorNumber,
            Aisle = b.ShelfLevel.Shelf.Zone.AisleCode,
            Rayon = b.ShelfLevel.Shelf.Name,
            Shelf = b.ShelfLevel.LevelNumber,

            // Tags
            Tags = b.BookTags.Select(bt => bt.Tag.Name).ToList()
        };

        #endregion

        #region Read Operations

        /// <summary>
        /// Retrieves a paginated list of books with sorting support.
        /// </summary>
        public async Task<(PagedResult<BookReadDto> Page, string? ETag, IActionResult? NotModified)>
            GetPagedAsync(int pageNumber, int pageSize, string sortBy, bool ascending, CancellationToken ct = default)
        {
            pageNumber = Math.Max(1, pageNumber);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var baseQuery = _db.Books.AsNoTracking();

            IOrderedQueryable<Book> ordered = (sortBy ?? "Title").ToLowerInvariant() switch
            {
                "bookid" or "id" => ascending ? baseQuery.OrderBy(b => b.BookId) : baseQuery.OrderByDescending(b => b.BookId),
                "title" => ascending ? baseQuery.OrderBy(b => b.Title) : baseQuery.OrderByDescending(b => b.Title),
                "isbn" => ascending ? baseQuery.OrderBy(b => b.Isbn) : baseQuery.OrderByDescending(b => b.Isbn),
                "publicationdate" or "publicationyear" => ascending ? baseQuery.OrderBy(b => b.PublicationDate) : baseQuery.OrderByDescending(b => b.PublicationDate),
                _ => ascending ? baseQuery.OrderBy(b => b.Title) : baseQuery.OrderByDescending(b => b.Title)
            };

            var totalCount = await baseQuery.CountAsync(ct);

            var items = await ordered
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(ReadProjection)
                .ToListAsync(ct);

            var page = PagedResult<BookReadDto>.Create(items, pageNumber, pageSize, totalCount);
            return (page, null, null);
        }

        /// <summary>
        /// Retrieves a single book by its identifier.
        /// </summary>
        public async Task<BookReadDto?> GetByIdAsync(int id, CancellationToken ct = default) =>
            await _db.Books
                .AsNoTracking()
                .Where(b => b.BookId == id)
                .Select(ReadProjection)
                .SingleOrDefaultAsync(ct);

        /// <summary>
        /// Searches for books based on a set of criteria (title, author, genre, etc.).
        /// </summary>
        public async Task<IEnumerable<BookReadDto>> SearchAsync(
            BookSearchDto dto,
            int? userId,
            CancellationToken ct = default)
        {
            var q = _db.Books.AsNoTracking().AsQueryable();

            // Filters applied dynamically
            if (!string.IsNullOrWhiteSpace(dto.Title))
            {
                var t = dto.Title.Trim();
                q = q.Where(b => EF.Functions.Like(b.Title, $"%{t}%"));
            }

            if (!string.IsNullOrWhiteSpace(dto.Author))
            {
                var a = dto.Author.Trim();
                q = q.Where(b => EF.Functions.Like(b.Author.Name, $"%{a}%"));
            }

            if (!string.IsNullOrWhiteSpace(dto.Genre))
            {
                var g = dto.Genre.Trim();
                q = q.Where(b => EF.Functions.Like(b.Genre.Name, $"%{g}%"));
            }

            if (!string.IsNullOrWhiteSpace(dto.Publisher))
            {
                var p = dto.Publisher.Trim();
                q = q.Where(b => EF.Functions.Like(b.Editor.Name, $"%{p}%"));
            }

            if (!string.IsNullOrWhiteSpace(dto.Isbn))
            {
                var i = dto.Isbn.Trim();
                q = q.Where(b => b.Isbn == i);
            }

            if (dto.YearMin.HasValue) q = q.Where(b => b.PublicationDate.Year >= dto.YearMin.Value);
            if (dto.YearMax.HasValue) q = q.Where(b => b.PublicationDate.Year <= dto.YearMax.Value);

            if (dto.IsAvailable.HasValue)
            {
                if (dto.IsAvailable.Value)
                {
                    q = q.Where(b =>
                        (
                            (_db.Stocks.Where(s => s.BookId == b.BookId)
                                .Select(s => (int?)s.Quantity).FirstOrDefault() ?? 0)
                          - (_db.Loans.Where(l => l.BookId == b.BookId && l.ReturnDate == null)
                                .Select(l => (int?)l.LoanId).Count())
                        ) > 0);
                }
                else
                {
                    q = q.Where(b =>
                        (
                            (_db.Stocks.Where(s => s.BookId == b.BookId)
                                .Select(s => (int?)s.Quantity).FirstOrDefault() ?? 0)
                          - (_db.Loans.Where(l => l.BookId == b.BookId && l.ReturnDate == null)
                                .Select(l => (int?)l.LoanId).Count())
                        ) <= 0);
                }
            }

            if (dto.TagIds is { Count: > 0 })
            {
                var ids = dto.TagIds.Distinct().ToArray();
                q = q.Where(b => b.BookTags.Any(bt => ids.Contains(bt.TagId)));
            }

            if (dto.TagNames is { Count: > 0 })
            {
                var names = dto.TagNames
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(s => s.Trim())
                    .ToArray();

                if (names.Length > 0)
                    q = q.Where(b => b.BookTags.Any(bt => names.Contains(bt.Tag.Name)));
            }

            if (!string.IsNullOrWhiteSpace(dto.Description))
            {
                var d = dto.Description.Trim();
                q = q.Where(b => b.Description != null && EF.Functions.Like(b.Description, $"%{d}%"));
            }

            if (!string.IsNullOrWhiteSpace(dto.Exclude))
            {
                var ex = dto.Exclude.Trim();
                q = q.Where(b =>
                    (b.Description == null || !EF.Functions.Like(b.Description, $"%{ex}%")) &&
                    !EF.Functions.Like(b.Title, $"%{ex}%"));
            }

            var results = await q.Select(ReadProjection).ToListAsync(ct);
            return results;
        }

        /// <summary>
        /// Retrieves the list of all genres in alphabetical order.
        /// </summary>
        public async Task<IReadOnlyList<string>> GetAllGenresAsync(CancellationToken ct = default) =>
            await _db.Genres.AsNoTracking()
                .OrderBy(g => g.Name)
                .Select(g => g.Name)
                .ToListAsync(ct);

        #endregion

        #region CRUD

        /// <summary>
        /// Creates a new book, its stock entry, and associated tags.
        /// </summary>
        public async Task<BookReadDto> CreateAsync(BookCreateDto dto, CancellationToken ct = default)
        {
            int shelfLevelId;
            if (dto.ShelfLevelId.HasValue)
            {
                shelfLevelId = dto.ShelfLevelId.Value;
            }
            else if (dto.Location is not null)
            {
                if (_location is null)
                    throw new InvalidOperationException("Location service is not configured, but Location payload was provided.");
                var loc = await _location.EnsureAsync(dto.Location, ct);
                shelfLevelId = loc.ShelfLevelId;
            }
            else
            {
                throw new ValidationException("Either ShelfLevelId or Location must be provided.");
            }

            var book = new Book
            {
                Title = dto.Title,
                Isbn = dto.Isbn,
                Description = dto.Description,
                PublicationDate = dto.PublicationDate,
                AuthorId = dto.AuthorId,
                GenreId = dto.GenreId,
                EditorId = dto.EditorId,
                ShelfLevelId = shelfLevelId,
                CoverUrl = dto.CoverUrl
            };

            _db.Books.Add(book);
            await _db.SaveChangesAsync(ct);

            // Tags
            if (dto.TagIds is { Count: > 0 })
            {
                foreach (var tagId in dto.TagIds.Distinct())
                    _db.BookTags.Add(new BookTag { BookId = book.BookId, TagId = tagId });

                await _db.SaveChangesAsync(ct);
            }

            // Stock
            var stock = await _db.Stocks.FirstOrDefaultAsync(s => s.BookId == book.BookId, ct);
            if (stock == null)
            {
                stock = new Stock { BookId = book.BookId, Quantity = dto.StockQuantity ?? 0 };
                _db.Stocks.Add(stock);
            }
            else if (dto.StockQuantity.HasValue)
            {
                stock.Quantity = dto.StockQuantity.Value;
                _db.Stocks.Update(stock);
            }

            if (_stockSvc != null) _stockSvc.UpdateAvailability(stock);
            await _db.SaveChangesAsync(ct);

            return await _db.Books.AsNoTracking()
                .Where(b => b.BookId == book.BookId)
                .Select(ReadProjection)
                .SingleAsync(ct);
        }

        /// <summary>
        /// Updates an existing book and its related tags/stock.
        /// </summary>
        public async Task<bool> UpdateAsync(int id, BookUpdateDto dto, CancellationToken ct = default)
        {
            var b = await _db.Books.FirstOrDefaultAsync(x => x.BookId == id, ct);
            if (b == null) return false;

            b.Title = dto.Title;
            b.Isbn = dto.Isbn;
            b.Description = dto.Description;
            b.PublicationDate = dto.PublicationDate;
            b.AuthorId = dto.AuthorId;
            b.GenreId = dto.GenreId;
            b.EditorId = dto.EditorId;
            b.ShelfLevelId = dto.ShelfLevelId;
            b.CoverUrl = dto.CoverUrl;

            // Tags
            if (dto.TagIds != null)
            {
                var current = _db.BookTags.Where(bt => bt.BookId == id);
                _db.BookTags.RemoveRange(current);

                foreach (var tagId in dto.TagIds.Distinct())
                    _db.BookTags.Add(new BookTag { BookId = id, TagId = tagId });
            }
            
            // Stock
            if (dto.StockQuantity.HasValue)
            {
                var stock = await _db.Stocks.FirstOrDefaultAsync(s => s.BookId == id, ct);
                if (stock != null)
                {
                    stock.Quantity = dto.StockQuantity.Value;
                    if (_stockSvc != null) _stockSvc.UpdateAvailability(stock);
                    _db.Stocks.Update(stock);
                }
            }

            await _db.SaveChangesAsync(ct);
            return true;
        }

        /// <summary>
        /// Deletes a book, its tags, and its stock record.
        /// </summary>
        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            var b = await _db.Books.FirstOrDefaultAsync(x => x.BookId == id, ct);
            if (b == null) return false;

            var tags = _db.BookTags.Where(bt => bt.BookId == id);
            _db.BookTags.RemoveRange(tags);

            var stock = await _db.Stocks.FirstOrDefaultAsync(s => s.BookId == id, ct);
            if (stock != null) _db.Stocks.Remove(stock);

            _db.Books.Remove(b);
            await _db.SaveChangesAsync(ct);
            return true;
        }

        #endregion
    }
}
