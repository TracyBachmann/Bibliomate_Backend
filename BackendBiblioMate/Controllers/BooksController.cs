using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Helpers;
using BackendBiblioMate.Models.Enums;
using BackendBiblioMate.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendBiblioMate.Controllers
{
    /// <summary>
    /// Controller for managing books.
    /// Provides CRUD operations, paged listing with ETag support,
    /// and filtered search with activity logging.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class BooksController : ControllerBase
    {
        private readonly IBookService _service;

        /// <summary>
        /// Initializes a new instance of <see cref="BooksController"/>.
        /// </summary>
        public BooksController(IBookService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        /// <summary>
        /// Retrieves a paged, sorted list of books with detailed information,
        /// using projection for performance and ETag support for caching.
        /// </summary>
        [AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<BookReadDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status304NotModified)]
        public async Task<IActionResult> GetBooks(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string sortBy = "Title",
            [FromQuery] bool ascending = true,
            CancellationToken cancellationToken = default)
        {
            var (page, eTag, notModified) =
                await _service.GetPagedAsync(pageNumber, pageSize, sortBy, ascending, cancellationToken);

            if (notModified != null)
                return notModified;

            Response.Headers["ETag"] = eTag!;
            return Ok(page);
        }

        /// <summary>
        /// Retrieves a single book by its identifier.
        /// </summary>
        [AllowAnonymous]
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BookReadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetBook(
            [FromRoute] int id,
            CancellationToken cancellationToken = default)
        {
            var dto = await _service.GetByIdAsync(id, cancellationToken);
            return dto == null ? NotFound() : Ok(dto);
        }

        /// <summary>
        /// Creates a new book record.
        /// Only accessible to Librarians and Admins.
        /// </summary>
        [Authorize(Roles = UserRoles.Librarian + "," + UserRoles.Admin)]
        [HttpPost]
        [ProducesResponseType(typeof(BookReadDto), StatusCodes.Status201Created)]
        public async Task<IActionResult> CreateBook(
            [FromBody] BookCreateDto dto,
            CancellationToken cancellationToken = default)
        {
            var created = await _service.CreateAsync(dto, cancellationToken);
            return CreatedAtAction(
                nameof(GetBook),
                new { id = created.BookId },
                created);
        }

        /// <summary>
        /// Updates an existing book.
        /// Only accessible to Librarians and Admins.
        /// </summary>
        [Authorize(Roles = UserRoles.Librarian + "," + UserRoles.Admin)]
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateBook(
            [FromRoute] int id,
            [FromBody] BookUpdateDto dto,
            CancellationToken cancellationToken = default)
        {
            if (!await _service.UpdateAsync(id, dto, cancellationToken))
                return NotFound();
            return NoContent();
        }

        /// <summary>
        /// Permanently deletes a book by its identifier.
        /// Only accessible to Librarians and Admins.
        /// </summary>
        [Authorize(Roles = UserRoles.Librarian + "," + UserRoles.Admin)]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteBook(
            [FromRoute] int id,
            CancellationToken cancellationToken = default)
        {
            if (!await _service.DeleteAsync(id, cancellationToken))
                return NotFound();
            return NoContent();
        }

        /// <summary>
        /// Searches books using multiple optional criteria.
        /// Accessible without authentication.
        /// </summary>
        [AllowAnonymous]
        [HttpPost("search")]
        [ProducesResponseType(typeof(IEnumerable<BookReadDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> SearchBooks(
            [FromBody] BookSearchDto dto,
            CancellationToken cancellationToken = default)
        {
            int? userId = null;
            if (User.Identity?.IsAuthenticated == true)
                userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var results = await _service.SearchAsync(dto, userId, cancellationToken);
            return Ok(results);
        }
    }
}