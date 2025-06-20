using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using backend.DTOs;
using Microsoft.AspNetCore.Authorization;
using backend.Models.Enums;

namespace backend.Controllers
{
    /// <summary>
    /// Controller for managing books.  
    /// Provides CRUD operations, advanced search,
    /// and detailed projections through DTOs.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class BooksController : ControllerBase
    {
        private readonly BiblioMateDbContext _context;

        public BooksController(BiblioMateDbContext context)
        {
            _context = context;
        }

        // GET: api/Books
        /// <summary>
        /// Retrieves all books with detailed information.
        /// </summary>
        /// <remarks>
        /// Includes related entities such as <see cref="Author"/>,
        /// <see cref="Genre"/>, <see cref="Editor"/>, shelf level,
        /// tags, and stock data.
        /// </remarks>
        /// <returns>A collection of <see cref="BookReadDto"/>.</returns>
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BookReadDto>>> GetBooks()
        {
            var books = await _context.Books
                .Include(b => b.Genre)
                .Include(b => b.Author)
                .Include(b => b.Editor)
                .Include(b => b.ShelfLevel)
                .Include(b => b.BookTags).ThenInclude(bt => bt.Tag)
                .Include(b => b.Stock)
                .ToListAsync();

            return Ok(books.Select(ToBookReadDto));
        }

        // GET: api/Books/{id}
        /// <summary>
        /// Retrieves a single book by its identifier.
        /// </summary>
        /// <param name="id">The book identifier.</param>
        /// <returns>
        /// The requested <see cref="BookReadDto"/> if found;
        /// otherwise <c>404 NotFound</c>.
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
        public async Task<ActionResult<Book>> CreateBook(BookCreateDto dto)
        {
            var book = new Book
            {
                Title = dto.Title,
                Isbn = dto.Isbn,
                PublicationDate = dto.PublicationDate,
                AuthorId = dto.AuthorId,
                GenreId = dto.GenreId,
                EditorId = dto.EditorId,
                ShelfLevelId = dto.ShelfLevelId,
                BookTags = (dto.TagIds ?? new List<int>())
                    .Select(tagId => new BookTag { TagId = tagId }).ToList()
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

            return CreatedAtAction(nameof(GetBook), new { id = book.BookId }, ToBookReadDto(book));
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

            book.Title          = dto.Title;
            book.Isbn           = dto.Isbn;
            book.PublicationDate = dto.PublicationDate;
            book.AuthorId       = dto.AuthorId;
            book.GenreId        = dto.GenreId;
            book.EditorId       = dto.EditorId;
            book.ShelfLevelId   = dto.ShelfLevelId;
            book.BookTags       = dto.TagIds?.Select(tagId =>
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
        private static BookReadDto ToBookReadDto(Book book) => new()
        {
            BookId         = book.BookId,
            Title          = book.Title,
            Isbn           = book.Isbn,
            PublicationYear = book.PublicationDate.Year,
            AuthorName     = book.Author.Name ?? "Unknown",
            GenreName      = book.Genre.Name ?? "Unknown",
            EditorName     = book.Editor.Name ?? "Unknown",
            IsAvailable    = book.Stock?.IsAvailable ?? false,
            Tags           = book.BookTags.Select(bt => bt.Tag.Name).ToList()
        };
    }
}
