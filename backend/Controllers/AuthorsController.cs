using backend.DTOs;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.Models.Enums;

namespace backend.Controllers
{
    /// <summary>
    /// Controller for managing authors.
    /// Provides CRUD endpoints for <see cref="AuthorReadDto"/>.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthorsController : ControllerBase
    {
        private readonly IAuthorService _service;

        /// <summary>
        /// Initializes a new instance of <see cref="AuthorsController"/>.
        /// </summary>
        /// <param name="service">Service encapsulating author logic.</param>
        public AuthorsController(IAuthorService service)
        {
            _service = service;
        }

        /// <summary>
        /// Retrieves all authors.
        /// </summary>
        /// <returns>200 OK with list of <see cref="AuthorReadDto"/>.</returns>
        [HttpGet, AllowAnonymous]
        public async Task<ActionResult<IEnumerable<AuthorReadDto>>> GetAuthors()
        {
            var items = await _service.GetAllAsync();
            return Ok(items);
        }

        /// <summary>
        /// Retrieves an author by its identifier.
        /// </summary>
        /// <param name="id">Author identifier.</param>
        /// <returns>
        /// 200 OK with <see cref="AuthorReadDto"/>, or 404 NotFound if missing.
        /// </returns>
        [HttpGet("{id}"), AllowAnonymous]
        public async Task<IActionResult> GetAuthor(int id)
        {
            var (dto, error) = await _service.GetByIdAsync(id);
            if (error != null) return error;
            return Ok(dto);
        }

        /// <summary>
        /// Creates a new author.
        /// </summary>
        /// <param name="dto">Data to create author.</param>
        /// <returns>
        /// 201 Created with location header, or 401/403 if unauthorized.
        /// </returns>
        [HttpPost, Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Librarian}")]
        public async Task<IActionResult> CreateAuthor(AuthorCreateDto dto)
        {
            var (_, result) = await _service.CreateAsync(dto);
            return result;
        }

        /// <summary>
        /// Updates an existing author.
        /// </summary>
        /// <param name="id">The author identifier.</param>
        /// <param name="dto">New author data.</param>
        /// <returns>
        /// 204 NoContent on success; 404 NotFound if missing.
        /// </returns>
        [HttpPut("{id}"), Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Librarian}")]
        public async Task<IActionResult> UpdateAuthor(int id, AuthorCreateDto dto)
        {
            if (!await _service.UpdateAsync(id, dto))
                return NotFound();
            return NoContent();
        }

        /// <summary>
        /// Deletes an author.
        /// </summary>
        /// <param name="id">The author identifier.</param>
        /// <returns>
        /// 204 NoContent on success; 404 NotFound if missing.
        /// </returns>
        [HttpDelete("{id}"), Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Librarian}")]
        public async Task<IActionResult> DeleteAuthor(int id)
        {
            if (!await _service.DeleteAsync(id))
                return NotFound();
            return NoContent();
        }
    }
}
