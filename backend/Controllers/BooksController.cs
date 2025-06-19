using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using backend.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace backend.Controllers
{
    /// <summary>
    /// Controller for managing books in the library catalog.
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
        /// Retrieves all books with detailed information (public access).
        /// </summary>
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BookReadDto>>> GetBooks()
        {
            var books = await _context.Books
                .Include(b => b.Genre)
                .Include(b => b.Author)
                .Include(b => b.Editor)
                .Include(b => b.ShelfLevel)
                .Include(b => b.BookTags)
                    .ThenInclude(bt => bt.Tag)
                .ToListAsync();

            var result = books.Select(book => new BookReadDto
            {
                BookId = book.BookId,
                Title = book.Title,
                Isbn = book.Isbn,
                PublicationYear = book.PublicationDate.Year,
                AuthorName = book.Author?.Name ?? "Unknown",
                GenreName = book.Genre?.Name ?? "Unknown",
                EditorName = book.Editor?.Name ?? "Unknown",
                Tags = book.BookTags.Select(bt => new TagDto
                {
                    TagId = bt.TagId,
                    Name = bt.Tag.Name
                }).ToList()
            });

            return Ok(result);
        }

        // GET: api/Books/{id}
        /// <summary>
        /// Retrieves a single book by ID.
        /// </summary>
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<BookReadDto>> GetBook(int id)
        {
            var book = await _context.Books
                .Include(b => b.Genre)
                .Include(b => b.Author)
                .Include(b => b.Editor)
                .Include(b => b.ShelfLevel)
                .Include(b => b.BookTags)
                    .ThenInclude(bt => bt.Tag)
                .FirstOrDefaultAsync(b => b.BookId == id);

            if (book == null)
                return NotFound();

            var dto = new BookReadDto
            {
                BookId = book.BookId,
                Title = book.Title,
                Isbn = book.Isbn,
                PublicationYear = book.PublicationDate.Year,
                AuthorName = book.Author?.Name ?? "Unknown",
                GenreName = book.Genre?.Name ?? "Unknown",
                EditorName = book.Editor?.Name ?? "Unknown",
                Tags = book.BookTags.Select(bt => new TagDto
                {
                    TagId = bt.TagId,
                    Name = bt.Tag.Name
                }).ToList()
            };

            return Ok(dto);
        }

        // POST: api/Books
        /// <summary>
        /// Creates a new book.
        /// </summary>
        [Authorize(Roles = "Librarian,Admin")]
        [HttpPost]
        public async Task<ActionResult<Book>> CreateBook(Book book)
        {
            _context.Books.Add(book);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetBook), new { id = book.BookId }, book);
        }

        // PUT: api/Books/{id}
        /// <summary>
        /// Updates an existing book.
        /// </summary>
        [Authorize(Roles = "Librarian,Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBook(int id, Book updatedBook)
        {
            if (id != updatedBook.BookId)
                return BadRequest("Book ID mismatch.");

            var existingBook = await _context.Books
                .Include(b => b.BookTags)
                .FirstOrDefaultAsync(b => b.BookId == id);

            if (existingBook == null)
                return NotFound();

            // Update scalar fields
            existingBook.Title = updatedBook.Title;
            existingBook.Isbn = updatedBook.Isbn;
            existingBook.PublicationDate = updatedBook.PublicationDate;
            existingBook.GenreId = updatedBook.GenreId;
            existingBook.AuthorId = updatedBook.AuthorId;
            existingBook.EditorId = updatedBook.EditorId;
            existingBook.ShelfLevelId = updatedBook.ShelfLevelId;

            // (Optionally) handle BookTags update logic here

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Books/{id}
        /// <summary>
        /// Deletes a book from the catalog.
        /// </summary>
        [Authorize(Roles = "Librarian,Admin")]
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
        /// Performs an advanced search for books.
        /// </summary>
        [HttpPost("search")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<BookReadDto>>> SearchBooks([FromBody] BookSearchDto dto)
        {
            var query = _context.Books
                .Include(b => b.Genre)
                .Include(b => b.Author)
                .Include(b => b.Editor)
                .Include(b => b.ShelfLevel)
                .Include(b => b.BookTags)
                    .ThenInclude(bt => bt.Tag)
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
                query = query.Where(b => b.PublicationDate.Year >= dto.YearMin.Value);

            if (dto.YearMax.HasValue)
                query = query.Where(b => b.PublicationDate.Year <= dto.YearMax.Value);

            if (dto.IsAvailable.HasValue)
            {
                if (dto.IsAvailable.Value)
                    query = query.Where(b => b.Stock != null && b.Stock.Quantity > 0);
                else
                    query = query.Where(b => b.Stock == null || b.Stock.Quantity == 0);
            }

            if (dto.TagIds != null && dto.TagIds.Any())
            {
                query = query.Where(b =>
                    b.BookTags.Any(bt => dto.TagIds.Contains(bt.TagId)));
            }

            var books = await query.ToListAsync();

            var results = books.Select(book => new BookReadDto
            {
                BookId = book.BookId,
                Title = book.Title,
                Isbn = book.Isbn,
                PublicationYear = book.PublicationDate.Year,
                AuthorName = book.Author?.Name ?? "Unknown",
                GenreName = book.Genre?.Name ?? "Unknown",
                EditorName = book.Editor?.Name ?? "Unknown",
                Tags = book.BookTags.Select(bt => new TagDto
                {
                    TagId = bt.TagId,
                    Name = bt.Tag.Name
                }).ToList()
            });

            return Ok(results);
        }
    }
}