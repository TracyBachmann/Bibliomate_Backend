using System.Security.Claims;
using backend.DTOs;
using backend.Helpers;
using backend.Models.Enums;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    /// <summary>
    /// Controller for managing books.
    /// Provides CRUD operations, paged listing with ETag support,
    /// and filtered search with activity logging.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class BooksController : ControllerBase
    {
        private readonly IBookService _service;

        /// <summary>
        /// Initializes a new instance of <see cref="BooksController"/>.
        /// </summary>
        /// <param name="service">Business service for book operations.</param>
        public BooksController(IBookService service)
        {
            _service = service;
        }

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
        /// <c>200 OK</c> with a <see cref="PagedResult{BookReadDto}"/> and ETag header,
        /// or <c>304 Not Modified</c> if the client’s ETag matches the server’s.
        /// </returns>
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetBooks(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize   = 20,
            [FromQuery] string sortBy   = "Title",
            [FromQuery] bool ascending  = true)
        {
            var (page, eTag, notModified) =
                await _service.GetPagedAsync(pageNumber, pageSize, sortBy, ascending);

            if (notModified != null)
                return notModified;

            Response.Headers["ETag"] = eTag;
            return Ok(page);
        }

        /// <summary>
        /// Retrieves a single book by its identifier.
        /// </summary>
        /// <param name="id">The book identifier.</param>
        /// <returns>
        /// The requested <see cref="BookReadDto"/> if found; otherwise <c>404 NotFound</c>.
        /// </returns>
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBook(int id)
        {
            var dto = await _service.GetByIdAsync(id);
            return dto == null ? NotFound() : Ok(dto);
        }

        /// <summary>
        /// Creates a new book record.
        /// Only accessible to Librarians and Admins.
        /// </summary>
        /// <param name="dto">The data required to create the book.</param>
        /// <returns>
        /// <c>201 Created</c> with the created DTO and a
        /// <c>Location</c> header pointing to <see cref="GetBook"/>.
        /// </returns>
        [Authorize(Roles = $"{UserRoles.Librarian},{UserRoles.Admin}")]
        [HttpPost]
        public async Task<IActionResult> CreateBook(BookCreateDto dto)
        {
            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetBook),
                                   new { id = created.BookId },
                                   created);
        }

        /// <summary>
        /// Updates an existing book.
        /// Only accessible to Librarians and Admins.
        /// </summary>
        /// <param name="id">Identifier of the book to update.</param>
        /// <param name="dto">The updated book data.</param>
        /// <returns>
        /// <c>204 NoContent</c> on success;  
        /// <c>400 BadRequest</c> if IDs mismatch;  
        /// <c>404 NotFound</c> if the book does not exist.
        /// </returns>
        [Authorize(Roles = $"{UserRoles.Librarian},{UserRoles.Admin}")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBook(int id, BookUpdateDto dto)
        {
            if (!await _service.UpdateAsync(id, dto))
                return NotFound();

            return NoContent();
        }

        /// <summary>
        /// Permanently deletes a book by its identifier.
        /// Only accessible to Librarians and Admins.
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
            if (!await _service.DeleteAsync(id))
                return NotFound();

            return NoContent();
        }

        /// <summary>
        /// Searches books using multiple optional criteria.
        /// Accessible without authentication.
        /// </summary>
        /// <param name="dto">The search filters.</param>
        /// <returns>
        /// <c>200 OK</c> with a filtered collection of <see cref="BookReadDto"/>.
        /// </returns>
        [AllowAnonymous]
        [HttpPost("search")]
        public async Task<IActionResult> SearchBooks([FromBody] BookSearchDto dto)
        {
            int? userId = null;
            if (User.Identity?.IsAuthenticated == true)
                userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var results = await _service.SearchAsync(dto, userId);
            return Ok(results);
        }
    }
}