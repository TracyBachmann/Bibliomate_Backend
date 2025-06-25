using System.Security.Claims;
using backend.Data;
using backend.DTOs;
using backend.Helpers;
using backend.Models;
using backend.Models.Enums;
using backend.Models.Mongo;          
using backend.Services;            
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers
{
    /// <summary>
    /// Controller for managing books.
    /// Provides CRUD operations, advanced search,
    /// and detailed projections through DTOs,
    /// with search activity logging and pagination.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class BooksController : ControllerBase
    {
        private readonly BiblioMateDbContext _context;
        private readonly SearchActivityLogService _searchLog;

        /// <summary>
        /// Initializes a new instance of <see cref="BooksController"/>.
        /// </summary>
        /// <param name="context">Database context for BiblioMate.</param>
        /// <param name="searchLog">Service to record search activity logs.</param>
        public BooksController(
            BiblioMateDbContext context,
            SearchActivityLogService searchLog)
        {
            _context   = context;
            _searchLog = searchLog;
        }

                // GET: api/Books
        /// <summary>
        /// Retrieves a paged, sorted list of books with detailed information,
        /// using projection for performance and ETag support for caching.
        /// </summary>
        /// <param name="pageNumber">Page number (1-based). Default = 1.</param>
        /// <param name="pageSize">Number of items per page. Default = 20.</param>
        /// <param name="sortBy">
        /// Field to sort by. Allowed values: "Title", "PublicationYear". Default = "Title".
        /// </param>
        /// <param name="ascending">Sort direction. True = ascending. Default = true.</param>
        /// <returns>
        /// <c>200 OK</c> with a <see cref="PagedResult{BookReadDto}"/> containing items and pagination metadata,
        /// or <c>304 Not Modified</c> if the client’s ETag matches the server’s.
        /// </returns>
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<PagedResult<BookReadDto>>> GetBooks(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string sortBy = "Title",
            [FromQuery] bool ascending = true)
        {
            // 1) Project directly into DTO to fetch only needed columns
            var baseQuery = _context.Books
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

            // 2) Apply dynamic sorting on DTO fields
            baseQuery = (sortBy, ascending) switch
            {
                ("PublicationYear", true)  => baseQuery.OrderBy(d => d.PublicationYear),
                ("PublicationYear", false) => baseQuery.OrderByDescending(d => d.PublicationYear),
                ("Title", false)           => baseQuery.OrderByDescending(d => d.Title),
                _                          => baseQuery.OrderBy(d => d.Title)
            };

            // 3) Compute total count before pagination
            var totalCount = await baseQuery.LongCountAsync();

            // 4) Apply paging
            var pageItems = await baseQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // 5) Build PagedResult
            var result = new PagedResult<BookReadDto>
            {
                PageNumber = pageNumber,
                PageSize   = pageSize,
                TotalCount = totalCount,
                Items      = pageItems
            };

            // 6) Generate simple ETag (MD5 hash of IDs + Titles)
            var eTagSource = string.Join(";", pageItems.Select(i => $"{i.BookId}-{i.Title}"));
            var eTagHash   = Convert.ToBase64String(
                System.Security.Cryptography.MD5
                    .Create()
                    .ComputeHash(System.Text.Encoding.UTF8.GetBytes(eTagSource))
            );
            var eTagValue = $"\"{eTagHash}\"";
            Response.Headers["ETag"] = eTagValue;

            // 7) Return 304 if client's ETag matches
            if (Request.Headers.TryGetValue("If-None-Match", out var clientETag) &&
                clientETag == eTagValue)
            {
                return StatusCode(StatusCodes.Status304NotModified);
            }

            // 8) Return paged result
            return Ok(result);
        }

        // GET: api/Books/{id}
        /// <summary>
        /// Retrieves a single book by its identifier.
        /// </summary>
        /// <param name="id">The book identifier.</param>
        /// <returns>
        /// The requested <see cref="BookReadDto"/> if found; otherwise <c>404 NotFound</c>.
        /// </returns>
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<BookReadDto>> GetBook(int id)
        {
            var book = await _context.Books
                .Include(b => b.Genre)
                .Include(b => b.Author)
                .Include(b => b.Editor)
                .Include(b => b.ShelfLevel)
                .Include(b => b.BookTags).ThenInclude(bt => bt.Tag)
                .Include(b => b.Stock)
                .FirstOrDefaultAsync(b => b.BookId == id);

            if (book == null)
                return NotFound();

            return Ok(ToBookReadDto(book));
        }

        // POST: api/Books
        /// <summary>
        /// Creates a new book record.
        /// </summary>
        /// <param name="dto">The data required to create the book.</param>
        /// <returns>
        /// <c>201 Created</c> with the created entity and a
        /// <c>Location</c> header pointing to <see cref="GetBook"/>;
        /// <c>409 Conflict</c> if the ISBN already exists.
        /// </returns>
        [Authorize(Roles = $"{UserRoles.Librarian},{UserRoles.Admin}")]
        [HttpPost]
        public async Task<ActionResult<BookReadDto>> CreateBook(BookCreateDto dto)
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
                BookTags        = (dto.TagIds ?? new List<int>())
                                    .Select(tagId => new BookTag { TagId = tagId })
                                    .ToList()
            };

            _context.Books.Add(book);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (
                ex.InnerException?.Message.Contains("IX_Books_Isbn") == true)
            {
                return Conflict("A book with this ISBN already exists.");
            }

            return CreatedAtAction(
                nameof(GetBook),
                new { id = book.BookId },
                ToBookReadDto(book)
            );
        }

        // PUT: api/Books/{id}
        /// <summary>
        /// Updates an existing book.
        /// </summary>
        /// <param name="id">The identifier of the book to update.</param>
        /// <param name="dto">The updated book data.</param>
        /// <returns>
        /// <c>204 NoContent</c> on success;  
        /// <c>400 BadRequest</c> if the IDs do not match;  
        /// <c>404 NotFound</c> if the book does not exist.
        /// </returns>
        [Authorize(Roles = $"{UserRoles.Librarian},{UserRoles.Admin}")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBook(int id, BookUpdateDto dto)
        {
            if (id != dto.BookId)
                return BadRequest("Book ID mismatch.");

            var book = await _context.Books
                .Include(b => b.BookTags)
                .FirstOrDefaultAsync(b => b.BookId == id);

            if (book == null)
                return NotFound();

            book.Title           = dto.Title;
            book.Isbn            = dto.Isbn;
            book.PublicationDate = dto.PublicationDate;
            book.AuthorId        = dto.AuthorId;
            book.GenreId         = dto.GenreId;
            book.EditorId        = dto.EditorId;
            book.ShelfLevelId    = dto.ShelfLevelId;
            book.BookTags        = dto.TagIds?.Select(tagId =>
                                       new BookTag { BookId = id, TagId = tagId })
                                       .ToList() ?? new List<BookTag>();

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Books/{id}
        /// <summary>
        /// Permanently deletes a book by its identifier.
        /// </summary>
        /// <param name="id">The identifier of the book to delete.</param>
        /// <returns>
        /// <c>204 NoContent</c> when deletion succeeds;  
        /// <c>404 NotFound</c> if the book is not found.
        /// </returns>
        [Authorize(Roles = $"{UserRoles.Librarian},{UserRoles.Admin}")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null)
                return NotFound();

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // POST: api/Books/search
        /// <summary>
        /// Searches books using multiple optional criteria.
        /// Accessible without authentication.
        /// </summary>
        /// <param name="dto">The search filters.</param>
        /// <returns>
        /// A filtered collection of <see cref="BookReadDto"/>.
        /// </returns>
        [HttpPost("search")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<BookReadDto>>> SearchBooks(
            [FromBody] BookSearchDto dto)
        {
            // Log the search activity
            int? userId = null;
            if (User.Identity?.IsAuthenticated == true)
                userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            await _searchLog.LogAsync(new SearchActivityLogDocument
            {
                UserId    = userId,
                QueryText = dto.ToString()!
            });

            // Build query
            var query = _context.Books
                .Include(b => b.Genre)
                .Include(b => b.Author)
                .Include(b => b.Editor)
                .Include(b => b.ShelfLevel)
                .Include(b => b.BookTags).ThenInclude(bt => bt.Tag)
                .Include(b => b.Stock)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(dto.Title))
                query = query.Where(b => b.Title.Contains(dto.Title));
            if (!string.IsNullOrWhiteSpace(dto.Author))
                query = query.Where(b => b.Author.Name.Contains(dto.Author));
            if (!string.IsNullOrWhiteSpace(dto.Genre))
                query = query.Where(b => b.Genre.Name.Contains(dto.Genre));
            if (!string.IsNullOrWhiteSpace(dto.Publisher))
                query = query.Where(b => b.Editor.Name.Contains(dto.Publisher));
            if (!string.IsNullOrWhiteSpace(dto.Isbn))
                query = query.Where(b => b.Isbn.Contains(dto.Isbn));
            if (dto.YearMin.HasValue)
                query = query.Where(b =>
                    b.PublicationDate >= new DateTime(dto.YearMin.Value, 1, 1));
            if (dto.YearMax.HasValue)
                query = query.Where(b =>
                    b.PublicationDate <= new DateTime(dto.YearMax.Value, 12, 31));
            if (dto.IsAvailable.HasValue)
                query = query.Where(b =>
                    b.Stock != null && b.Stock.IsAvailable == dto.IsAvailable.Value);
            if (dto.TagIds is { Count: > 0 })
                query = query.Where(b =>
                    b.BookTags.Any(bt => dto.TagIds.Contains(bt.TagId)));

            var books = await query.ToListAsync();
            return Ok(books.Select(ToBookReadDto));
        }

        // (private) — helper
        /// <summary>
        /// Maps a <see cref="Book"/> entity to its read-side DTO.
        /// </summary>
        /// <param name="book">The <see cref="Book"/> entity.</param>
        /// <returns>The corresponding <see cref="BookReadDto"/>.</returns>
        private static BookReadDto ToBookReadDto(Book book) => new()
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
