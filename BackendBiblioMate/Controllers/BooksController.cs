using System.Security.Claims;
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
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>200 OK</c> with a <see cref="PagedResult{T}"/> and ETag header,  
        /// or <c>304 Not Modified</c> if the client’s ETag matches the server’s.
        /// </returns>
        [AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<BookReadDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status304NotModified)]
        public async Task<IActionResult> GetBooks(
            [FromQuery] int pageNumber   = 1,
            [FromQuery] int pageSize     = 20,
            [FromQuery] string sortBy    = "Title",
            [FromQuery] bool ascending   = true,
            CancellationToken cancellationToken = default)
        {
            var (page, eTag, notModified) =
                await _service.GetPagedAsync(pageNumber, pageSize, sortBy, ascending, cancellationToken);

            if (notModified != null)
                return notModified;

            Response.Headers["ETag"] = eTag;
            return Ok(page);
        }

        /// <summary>
        /// Retrieves a single book by its identifier.
        /// </summary>
        /// <param name="id">The book identifier.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>200 OK</c> with <see cref="BookReadDto"/>,  
        /// <c>404 NotFound</c> if not found.
        /// </returns>
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
        /// <param name="dto">The data required to create the book.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>201 Created</c> with the created DTO and a
        /// <c>Location</c> header pointing to <see cref="GetBook"/>.
        /// </returns>
        [Authorize(Roles = UserRoles.Librarian + "," + UserRoles.Admin)]
        [HttpPost]
        [ProducesResponseType(typeof(BookReadDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
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
        /// <param name="id">Identifier of the book to update.</param>
        /// <param name="dto">The updated book data.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>204 NoContent</c> on success;  
        /// <c>404 NotFound</c> if the book does not exist.
        /// </returns>
        [Authorize(Roles = UserRoles.Librarian + "," + UserRoles.Admin)]
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
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
        /// <param name="id">The identifier of the book to delete.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>204 NoContent</c> when deletion succeeds;  
        /// <c>404 NotFound</c> if the book is not found.
        /// </returns>
        [Authorize(Roles = UserRoles.Librarian + "," + UserRoles.Admin)]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
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
        /// <param name="dto">The search filters.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>200 OK</c> with a filtered collection of <see cref="BookReadDto"/>.
        /// </returns>
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