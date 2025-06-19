using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using backend.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace backend.Controllers
{
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

        /// <summary>
        /// Retrieves all books with detailed information.
        /// </summary>
        /// <returns>List of all books with their metadata.</returns>
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

        /// <summary>
        /// Retrieves a book by ID.
        /// </summary>
        /// <param name="id">Book ID.</param>
        /// <returns>BookReadDto for the specified book.</returns>
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

        /// <summary>
        /// Creates a new book.
        /// </summary>
        /// <param name="dto">Data for the new book.</param>
        /// <returns>The created book.</returns>
        [Authorize(Roles = "Librarian,Admin")]
        [HttpPost]
        public async Task<ActionResult<Book>> CreateBook(BookCreateDTO dto)
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
                BookTags = dto.TagIds?.Select(tagId => new BookTag { TagId = tagId }).ToList()
            };

            _context.Books.Add(book);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException?.Message.Contains("IX_Books_Isbn") == true)
                    return Conflict("Un livre avec cet ISBN existe déjà.");
                throw;
            }

            return CreatedAtAction(nameof(GetBook), new { id = book.BookId }, book);
        }

        /// <summary>
        /// Updates an existing book.
        /// </summary>
        /// <param name="id">ID of the book to update.</param>
        /// <param name="dto">Updated book data.</param>
        [Authorize(Roles = "Librarian,Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBook(int id, BookUpdateDTO dto)
        {
            if (id != dto.BookId)
                return BadRequest("Mismatch de l'ID du livre.");

            var book = await _context.Books
                .Include(b => b.BookTags)
                .FirstOrDefaultAsync(b => b.BookId == id);

            if (book == null)
                return NotFound();

            book.Title = dto.Title;
            book.Isbn = dto.Isbn;
            book.PublicationDate = dto.PublicationDate;
            book.AuthorId = dto.AuthorId;
            book.GenreId = dto.GenreId;
            book.EditorId = dto.EditorId;
            book.ShelfLevelId = dto.ShelfLevelId;
            book.BookTags = dto.TagIds?.Select(tagId => new BookTag { BookId = id, TagId = tagId }).ToList()
                            ?? new List<BookTag>();

            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Deletes a book by ID.
        /// </summary>
        /// <param name="id">ID of the book to delete.</param>
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

        /// <summary>
        /// Searches books with multiple criteria.
        /// </summary>
        /// <param name="dto">Search filters.</param>
        /// <returns>Filtered list of books.</returns>
        [HttpPost("search")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<BookReadDto>>> SearchBooks([FromBody] BookSearchDto dto)
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
                query = query.Where(b => b.PublicationDate >= new DateTime(dto.YearMin.Value, 1, 1));

            if (dto.YearMax.HasValue)
                query = query.Where(b => b.PublicationDate <= new DateTime(dto.YearMax.Value, 12, 31));

            if (dto.IsAvailable.HasValue)
                query = query.Where(b => b.Stock != null && b.Stock.IsAvailable == dto.IsAvailable.Value);

            if (dto.TagIds is { Count: > 0 })
                query = query.Where(b => b.BookTags.Any(bt => dto.TagIds.Contains(bt.TagId)));

            var books = await query.ToListAsync();
            return Ok(books.Select(ToBookReadDto));
        }

        /// <summary>
        /// Maps a Book entity to BookReadDto.
        /// </summary>
        private static BookReadDto ToBookReadDto(Book book) => new()
        {
            BookId = book.BookId,
            Title = book.Title,
            Isbn = book.Isbn,
            PublicationYear = book.PublicationDate.Year,
            AuthorName = book.Author?.Name ?? "Inconnu",
            GenreName = book.Genre?.Name ?? "Inconnu",
            EditorName = book.Editor?.Name ?? "Inconnu",
            IsAvailable = book.Stock?.IsAvailable ?? false,
            Tags = book.BookTags.Select(bt => bt.Tag.Name).ToList()
        };
    }
}
