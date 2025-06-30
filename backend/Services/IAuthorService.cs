using backend.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace backend.Services
{
    /// <summary>
    /// Encapsulates business logic for managing authors.
    /// </summary>
    public interface IAuthorService
    {
        /// <summary>
        /// Retrieves all authors.
        /// </summary>
        /// <returns>A list of <see cref="AuthorReadDto"/>.</returns>
        Task<IEnumerable<AuthorReadDto>> GetAllAsync();

        /// <summary>
        /// Retrieves a single author by its identifier.
        /// </summary>
        /// <param name="id">The author identifier.</param>
        /// <returns>
        /// A tuple: the <see cref="AuthorReadDto"/> (if found) and
        /// an <see cref="IActionResult"/> to return on error.
        /// </returns>
        Task<(AuthorReadDto? Dto, IActionResult? ErrorResult)> GetByIdAsync(int id);

        /// <summary>
        /// Creates a new author.
        /// </summary>
        /// <param name="dto">The author creation data.</param>
        /// <returns>
        /// A tuple: the created <see cref="AuthorReadDto"/> and its
        /// <see cref="CreatedAtActionResult"/> wrapper.
        /// </returns>
        Task<(AuthorReadDto Dto, CreatedAtActionResult Result)> CreateAsync(AuthorCreateDto dto);

        /// <summary>
        /// Updates an existing author.
        /// </summary>
        /// <param name="id">The author identifier.</param>
        /// <param name="dto">The new author data.</param>
        /// <returns>
        /// <c>true</c> if update succeeded; otherwise <c>false</c>.
        /// </returns>
        Task<bool> UpdateAsync(int id, AuthorCreateDto dto);

        /// <summary>
        /// Deletes an author by its identifier.
        /// </summary>
        /// <param name="id">The author identifier.</param>
        /// <returns>
        /// <c>true</c> if deletion succeeded; otherwise <c>false</c>.
        /// </returns>
        Task<bool> DeleteAsync(int id);
    }
}
