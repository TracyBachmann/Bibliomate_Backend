using backend.Data;
using backend.DTOs;
using backend.Helpers;
using backend.Models;
using backend.Models.Mongo;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;


namespace backend.Services
{
    /// <summary>
    /// Implements <see cref="IBookService"/> by coordinating EF Core, projection to DTO,
    /// pagination, sorting, ETag generation, and search‐activity logging.
    /// </summary>
    public class BookService : IBookService
    {
        private readonly BiblioMateDbContext      _db;
        private readonly ISearchActivityLogService _searchLog;
        
        /// <summary>
        /// Initializes a new instance of <see cref="BookService"/>.
        /// </summary>
        /// <param name="db">EF Core database context.</param>
        /// <param name="searchLog">Service to record search activity.</param>
        public BookService(
            BiblioMateDbContext      db,
            ISearchActivityLogService searchLog)
        {
            _db        = db;
            _searchLog = searchLog;
        }

        /// <inheritdoc/>
        public async Task<(PagedResult<BookReadDto> Page, string ETag, IActionResult? NotModified)>
            GetPagedAsync(int pageNumber, int pageSize, string sortBy, bool ascending)
        {
            // Project only needed columns
            var query = _db.Books
                .Select(b => new BookReadDto
                {
                    BookId          = b.BookId,
                    Title           = b.Title,
                    Isbn            = b.Isbn,
                    PublicationYear = b.PublicationDate.Year,
                    AuthorName      = b.Author.Name,
                    GenreName       = b.Genre.Name,
                    EditorName      = b.Editor.Name,
                    IsAvailable     = b.Stock != null && b.Stock.IsAvailable,
                    CoverUrl        = b.CoverUrl,
                    Tags            = b.BookTags.Select(bt => bt.Tag.Name).ToList()
                });

            // Dynamic sorting
            query = (sortBy, ascending) switch
            {
                ("PublicationYear", true)  => query.OrderBy(d => d.PublicationYear),
                ("PublicationYear", false) => query.OrderByDescending(d => d.PublicationYear),
                ("Title", false)           => query.OrderByDescending(d => d.Title),
                _                          => query.OrderBy(d => d.Title)
            };

            // Total count
            var totalCount = await query.LongCountAsync();

            // Page items
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Build result
            var page = new PagedResult<BookReadDto>
            {
                PageNumber = pageNumber,
                PageSize   = pageSize,
                TotalCount = totalCount,
                Items      = items
            };

            // ETag = base64(MD5 of "id-title;…")
            var eTagSource = string.Join(";", items.Select(i => $"{i.BookId}-{i.Title}"));
            var hash       = MD5.HashData(Encoding.UTF8.GetBytes(eTagSource));
            var eTag       = $"\"{Convert.ToBase64String(hash)}\"";

            // Caller will set Response.Headers["ETag"] = eTag
            return (page, eTag, null);
        }

        /// <inheritdoc/>
        public async Task<BookReadDto?> GetByIdAsync(int id)
        {
            var b = await _db.Books
                .Include(b => b.Genre)
                .Include(b => b.Author)
                .Include(b => b.Editor)
                .Include(b => b.ShelfLevel)
                .Include(b => b.BookTags).ThenInclude(bt => bt.Tag)
                .Include(b => b.Stock)
                .FirstOrDefaultAsync(b => b.BookId == id);

            return b == null ? null : ToDto(b);
        }

        /// <inheritdoc/>
        public async Task<BookReadDto> CreateAsync(BookCreateDto dto)
        {
            var book = new Book
            {
                Title           = dto.Title,
                Isbn            = dto.Isbn,
                PublicationDate = dto.PublicationDate,
                AuthorId        = dto.AuthorId,
                GenreId         = dto.GenreId,
                EditorId        = dto.EditorId,
                ShelfLevelId    = dto.ShelfLevelId,
                CoverUrl        = dto.CoverUrl,
                BookTags        = (dto.TagIds ?? new List<int>())
                    .Select(tagId => new BookTag { TagId = tagId })
                    .ToList()
            };

            _db.Books.Add(book);
            await _db.SaveChangesAsync();

            await _db.Entry(book).Reference(b => b.Author).LoadAsync();
            await _db.Entry(book).Reference(b => b.Genre).LoadAsync();
            await _db.Entry(book).Reference(b => b.Editor).LoadAsync();
            await _db.Entry(book).Reference(b => b.ShelfLevel).LoadAsync();
            await _db.Entry(book).Collection(b => b.BookTags).Query().Include(bt => bt.Tag).LoadAsync();
            await _db.Entry(book).Reference(b => b.Stock).LoadAsync();

            return ToDto(book);
        }

        /// <inheritdoc/>
        public async Task<bool> UpdateAsync(int id, BookUpdateDto dto)
        {
            if (id != dto.BookId) return false;

            var book = await _db.Books
                .Include(b => b.BookTags)
                .FirstOrDefaultAsync(b => b.BookId == id);

            if (book == null) return false;

            book.Title           = dto.Title;
            book.Isbn            = dto.Isbn;
            book.PublicationDate = dto.PublicationDate;
            book.AuthorId        = dto.AuthorId;
            book.GenreId         = dto.GenreId;
            book.EditorId        = dto.EditorId;
            book.ShelfLevelId    = dto.ShelfLevelId;
            book.BookTags = dto.TagIds?.Select(tagId =>
                new BookTag { BookId = id, TagId = tagId }).ToList()
                ?? new List<BookTag>();

            await _db.SaveChangesAsync();
            return true;
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(int id)
        {
            var book = await _db.Books.FindAsync(id);
            if (book == null) return false;

            _db.Books.Remove(book);
            await _db.SaveChangesAsync();
            return true;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<BookReadDto>> SearchAsync(BookSearchDto dto, int? userId)
        {
            // Log the search
            await _searchLog.LogAsync(new SearchActivityLogDocument
            {
                UserId    = userId,
                QueryText = dto.ToString()!
            });

            // Build EF query
            var q = _db.Books
                .Include(b => b.Genre)
                .Include(b => b.Author)
                .Include(b => b.Editor)
                .Include(b => b.ShelfLevel)
                .Include(b => b.BookTags).ThenInclude(bt => bt.Tag)
                .Include(b => b.Stock)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(dto.Title))
                q = q.Where(b => b.Title.Contains(dto.Title));
            if (!string.IsNullOrWhiteSpace(dto.Author))
                q = q.Where(b => b.Author.Name.Contains(dto.Author));
            if (!string.IsNullOrWhiteSpace(dto.Genre))
                q = q.Where(b => b.Genre.Name.Contains(dto.Genre));
            if (!string.IsNullOrWhiteSpace(dto.Publisher))
                q = q.Where(b => b.Editor.Name.Contains(dto.Publisher));
            if (!string.IsNullOrWhiteSpace(dto.Isbn))
                q = q.Where(b => b.Isbn.Contains(dto.Isbn));
            if (dto.YearMin.HasValue)
                q = q.Where(b => b.PublicationDate >= new DateTime(dto.YearMin.Value, 1, 1));
            if (dto.YearMax.HasValue)
                q = q.Where(b => b.PublicationDate <= new DateTime(dto.YearMax.Value, 12, 31));
            if (dto.IsAvailable.HasValue)
                q = q.Where(b => b.Stock != null && b.Stock.IsAvailable == dto.IsAvailable.Value);
            if (dto.TagIds is { Count: > 0 })
                q = q.Where(b => b.BookTags.Any(bt => dto.TagIds.Contains(bt.TagId)));

            var list = await q.ToListAsync();
            return list.Select(ToDto);
        }

        /// <summary>
        /// Maps a <see cref="Book"/> entity to its corresponding <see cref="BookReadDto"/>.
        /// </summary>
        private static BookReadDto ToDto(Book book) => new()
        {
            BookId          = book.BookId,
            Title           = book.Title,
            Isbn            = book.Isbn,
            PublicationYear = book.PublicationDate.Year,
            AuthorName      = book.Author.Name,
            GenreName       = book.Genre.Name,
            EditorName      = book.Editor.Name,
            IsAvailable     = book.Stock?.IsAvailable ?? false,
            Tags            = book.BookTags.Select(bt => bt.Tag.Name).ToList()
        };
    }
}