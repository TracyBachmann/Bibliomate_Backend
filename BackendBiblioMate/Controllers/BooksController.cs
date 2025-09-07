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
    [ApiController]
    [Route("api/[controller]")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    public class BooksController : ControllerBase
    {
        private readonly IBookService _service;
        public BooksController(IBookService service) => _service = service ?? throw new ArgumentNullException(nameof(service));

        /// <summary>Retrieves a paged, sorted list of books (ETag enabled).</summary>
        [AllowAnonymous]
        [HttpGet]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(Summary = "Retrieves a paged list of books (v1)", Tags = ["Books"])]
        [ProducesResponseType(typeof(PagedResult<BookReadDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status304NotModified)]
        public async Task<IActionResult> GetBooks(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string sortBy = "Title",
            [FromQuery] bool ascending = true,
            CancellationToken cancellationToken = default)
        {
            var (page, eTag, notModified) = await _service.GetPagedAsync(pageNumber, pageSize, sortBy, ascending, cancellationToken);
            if (notModified != null) return notModified;
            if (!string.IsNullOrEmpty(eTag)) Response.Headers["ETag"] = eTag;
            return Ok(page);
        }

        /// <summary>Retrieves a single book by its identifier.</summary>
        [AllowAnonymous]
        [HttpGet("{id:int}")]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(Summary = "Retrieves a book by ID (v1)", Tags = ["Books"])]
        [ProducesResponseType(typeof(BookReadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetBook([FromRoute] int id, CancellationToken cancellationToken = default)
        {
            var dto = await _service.GetByIdAsync(id, cancellationToken);
            return dto == null ? NotFound() : Ok(dto);
        }

        /// <summary>Creates a new book (Librarian/Admin).</summary>
        [Authorize(Roles = UserRoles.Librarian + "," + UserRoles.Admin)]
        [HttpPost]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(Summary = "Creates a new book (v1)", Tags = ["Books"])]
        [ProducesResponseType(typeof(BookReadDto), StatusCodes.Status201Created)]
        public async Task<IActionResult> CreateBook([FromBody] BookCreateDto dto, CancellationToken cancellationToken = default)
        {
            var created = await _service.CreateAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetBook), new { id = created.BookId }, created);
        }

        /// <summary>Updates an existing book (Librarian/Admin).</summary>
        [Authorize(Roles = UserRoles.Librarian + "," + UserRoles.Admin)]
        [HttpPut("{id:int}")]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(Summary = "Updates an existing book (v1)", Tags = ["Books"])]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateBook([FromRoute] int id, [FromBody] BookUpdateDto dto, CancellationToken cancellationToken = default)
        {
            if (!await _service.UpdateAsync(id, dto, cancellationToken)) return NotFound();
            return NoContent();
        }

        /// <summary>Deletes a book by ID (Librarian/Admin).</summary>
        [Authorize(Roles = UserRoles.Librarian + "," + UserRoles.Admin)]
        [HttpDelete("{id:int}")]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(Summary = "Deletes a book by ID (v1)", Tags = ["Books"])]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteBook([FromRoute] int id, CancellationToken cancellationToken = default)
        {
            if (!await _service.DeleteAsync(id, cancellationToken)) return NotFound();
            return NoContent();
        }

        /// <summary>Searches books with multiple optional criteria.</summary>
        [AllowAnonymous]
        [HttpPost("search")]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(Summary = "Searches books with filters (v1)", Tags = ["Books"])]
        [ProducesResponseType(typeof(IEnumerable<BookReadDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> SearchBooks([FromBody] BookSearchDto dto, CancellationToken cancellationToken = default)
        {
            int? userId = null;
            if (User.Identity?.IsAuthenticated == true)
                userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var results = await _service.SearchAsync(dto, userId, cancellationToken);
            return Ok(results);
        }

        /// <summary>Returns all genre names.</summary>
        [AllowAnonymous]
        [HttpGet("genres")]
        [MapToApiVersion("1.0")]
        public async Task<IActionResult> GetGenres(CancellationToken ct = default)
        {
            var list = await _service.GetAllGenresAsync(ct);
            return Ok(list);
        }
    }
}