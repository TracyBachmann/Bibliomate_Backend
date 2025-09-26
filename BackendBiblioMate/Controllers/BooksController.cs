using System.Security.Claims;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Helpers;
using BackendBiblioMate.Models.Enums;
using BackendBiblioMate.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace BackendBiblioMate.Controllers
{
    /// <summary>
    /// API controller for managing books.
    /// Provides endpoints for retrieval, creation, update, deletion, and search with filters.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    public class BooksController : ControllerBase
    {
        private readonly IBookService _service;

        /// <summary>
        /// Initializes a new instance of the <see cref="BooksController"/> class.
        /// </summary>
        /// <param name="service">The service used to manage book entities.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="service"/> is null.</exception>
        public BooksController(IBookService service) =>
            _service = service ?? throw new ArgumentNullException(nameof(service));

        /// <summary>
        /// Retrieves a paged and sorted list of books.
        /// Supports ETag-based conditional requests to optimize performance.
        /// </summary>
        /// <param name="pageNumber">The current page number. Defaults to 1.</param>
        /// <param name="pageSize">The number of items per page. Defaults to 20.</param>
        /// <param name="sortBy">The property name used for sorting. Defaults to "Title".</param>
        /// <param name="ascending">Whether the sorting order is ascending. Defaults to true.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>200 OK</c> with a paged result of <see cref="BookReadDto"/>,  
        /// <c>304 Not Modified</c> if the ETag matches and the data has not changed.
        /// </returns>
        [AllowAnonymous]
        [HttpGet]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Retrieves a paged list of books (v1)",
            Description = "Supports pagination, sorting, and ETag-based caching.",
            Tags = ["Books"]
        )]
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

            if (!string.IsNullOrEmpty(eTag))
                Response.Headers["ETag"] = eTag;

            return Ok(page);
        }

        /// <summary>
        /// Retrieves a single book by its unique identifier.
        /// </summary>
        /// <param name="id">The book identifier.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>200 OK</c> with the <see cref="BookReadDto"/> if found,  
        /// <c>404 Not Found</c> if the book does not exist.
        /// </returns>
        [AllowAnonymous]
        [HttpGet("{id:int}")]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Retrieves a book by ID (v1)",
            Description = "Fetches a single book by its unique identifier.",
            Tags = ["Books"]
        )]
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
        /// Creates a new book entry. Requires Librarian or Admin role.
        /// </summary>
        /// <param name="dto">The data required to create the book.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>201 Created</c> with the created <see cref="BookReadDto"/> and its location.
        /// </returns>
        [Authorize(Roles = UserRoles.Librarian + "," + UserRoles.Admin)]
        [HttpPost]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Creates a new book (v1)",
            Description = "Accessible only to Librarians and Admins.",
            Tags = ["Books"]
        )]
        [ProducesResponseType(typeof(BookReadDto), StatusCodes.Status201Created)]
        public async Task<IActionResult> CreateBook(
            [FromBody] BookCreateDto dto,
            CancellationToken cancellationToken = default)
        {
            var created = await _service.CreateAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetBook), new { id = created.BookId }, created);
        }

        /// <summary>
        /// Updates an existing book entry. Requires Librarian or Admin role.
        /// </summary>
        /// <param name="id">The identifier of the book to update.</param>
        /// <param name="dto">The updated book data.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>204 No Content</c> if the update was successful,  
        /// <c>404 Not Found</c> if the book does not exist.
        /// </returns>
        [Authorize(Roles = UserRoles.Librarian + "," + UserRoles.Admin)]
        [HttpPut("{id:int}")]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Updates an existing book (v1)",
            Description = "Accessible only to Librarians and Admins.",
            Tags = ["Books"]
        )]
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
        /// Deletes a book entry by its unique identifier. Requires Librarian or Admin role.
        /// </summary>
        /// <param name="id">The identifier of the book to delete.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>204 No Content</c> if the deletion was successful,  
        /// <c>404 Not Found</c> if the book does not exist.
        /// </returns>
        [Authorize(Roles = UserRoles.Librarian + "," + UserRoles.Admin)]
        [HttpDelete("{id:int}")]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Deletes a book by ID (v1)",
            Description = "Accessible only to Librarians and Admins.",
            Tags = ["Books"]
        )]
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
        /// Searches books with multiple optional filters.
        /// If the user is authenticated, the user ID is included for personalized results.
        /// </summary>
        /// <param name="dto">The search criteria (title, author, genre, etc.).</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns><c>200 OK</c> with the list of matching books.</returns>
        [AllowAnonymous]
        [HttpPost("search")]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Searches books with filters (v1)",
            Description = "Supports multiple filters and personalization if user is authenticated.",
            Tags = ["Books"]
        )]
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

        /// <summary>
        /// Retrieves the list of all available genre names.
        /// </summary>
        /// <param name="ct">Token to monitor for cancellation requests.</param>
        /// <returns><c>200 OK</c> with a list of genre names.</returns>
        [AllowAnonymous]
        [HttpGet("genres")]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Retrieves all genre names (v1)",
            Description = "Returns a flat list of genre names available in the system.",
            Tags = ["Books"]
        )]
        [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetGenres(CancellationToken ct = default)
        {
            var list = await _service.GetAllGenresAsync(ct);
            return Ok(list);
        }
    }
}
