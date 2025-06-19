using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using Microsoft.AspNetCore.Authorization;

namespace backend.Controllers
{
    /// <summary>
    /// Controller for managing books in the library catalog.
    /// Publicly accessible for viewing books.
    /// Restricted access for creation, modification, and deletion.
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
        /// Retrieves the list of all books with their genres and shelf levels.
        /// </summary>
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Book>>> GetBooks()
        {
            return await _context.Books
                .Include(b => b.Genre)
                .Include(b => b.ShelfLevel)
                .ToListAsync();
        }

        // GET: api/Books/{id}
        /// <summary>
        /// Retrieves details of a single book by ID, including genre and shelf level.
        /// </summary>
        /// <param name="id">ID of the book.</param>
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<Book>> GetBook(int id)
        {
            var book = await _context.Books
                .Include(b => b.Genre)
                .Include(b => b.ShelfLevel)
                .FirstOrDefaultAsync(b => b.BookId == id);

            if (book == null)
                return NotFound();

            return book;
        }

        // POST: api/Books
        /// <summary>
        /// Creates a new book entry.
        /// Only accessible to Librarians and Admins.
        /// </summary>
        /// <param name="book">Book data to create.</param>
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
        /// Only accessible to Librarians and Admins.
        /// </summary>
        /// <param name="id">ID of the book to update.</param>
        /// <param name="book">Updated book data.</param>
        [Authorize(Roles = "Librarian,Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBook(int id, Book book)
        {
            if (id != book.BookId)
                return BadRequest("Mismatch avec l'ID du livre.");

            _context.Entry(book).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Books.Any(b => b.BookId == id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // DELETE: api/Books/{id}
        /// <summary>
        /// Deletes a book from the catalog.
        /// Only accessible to Librarians and Admins.
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
    }
}