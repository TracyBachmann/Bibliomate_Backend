using BackendBiblioMate.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace BackendBiblioMate.Interfaces
{
    /// <summary>
    /// Encapsulates business logic for managing authors.
    /// Provides methods for CRUD operations on author data.
    /// </summary>
    public interface IAuthorService
    {
        /// <summary>
        /// Retrieves all authors.
        /// </summary>
        /// <param name="cancellationToken">
        /// Token to monitor for cancellation requests.
        /// </param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> that, when completed successfully,
        /// yields an <see cref="IEnumerable{AuthorReadDto}"/> with all authors.
        /// </returns>
        Task<IEnumerable<AuthorReadDto>> GetAllAsync(
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a single author by its identifier.
        /// </summary>
        /// <param name="id">The identifier of the author to retrieve.</param>
        /// <param name="cancellationToken">
        /// Token to monitor for cancellation requests.
        /// </param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> that, when completed,
        /// yields a tuple containing:
        /// <list type="bullet">
        ///   <item>
        ///     <description>
        ///       <see cref="AuthorReadDto"/> if found;
        ///       otherwise <c>null</c>.
        ///     </description>
        ///   </item>
        ///   <item>
        ///     <description>
        ///       An <see cref="IActionResult"/> representing an error response
        ///       (e.g. <c>NotFound</c>), or <c>null</c> on success.
        ///     </description>
        ///   </item>
        /// </list>
        /// </returns>
        Task<(AuthorReadDto? Dto, IActionResult? ErrorResult)> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new author.
        /// </summary>
        /// <param name="dto">Data for the author to create.</param>
        /// <param name="cancellationToken">
        /// Token to monitor for cancellation requests.
        /// </param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> that, when completed,
        /// yields a tuple containing:
        /// <list type="bullet">
        ///   <item>
        ///     <description>
        ///       The created <see cref="AuthorReadDto"/>.
        ///     </description>
        ///   </item>
        ///   <item>
        ///     <description>
        ///       A <see cref="CreatedAtActionResult"/> with
        ///       the location header set to the new resource.
        ///     </description>
        ///   </item>
        /// </list>
        /// </returns>
        Task<(AuthorReadDto Dto, CreatedAtActionResult Result)> CreateAsync(
            AuthorCreateDto dto,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing author.
        /// </summary>
        /// <param name="id">Identifier of the author to update.</param>
        /// <param name="dto">New values for the author.</param>
        /// <param name="cancellationToken">
        /// Token to monitor for cancellation requests.
        /// </param>
        /// <returns>
        /// A <see cref="Task{Boolean}"/> that, when completed,
        /// returns <c>true</c> if the update succeeded; <c>false</c> if the author was not found.
        /// </returns>
        Task<bool> UpdateAsync(
            int id,
            AuthorCreateDto dto,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes an author by its identifier.
        /// </summary>
        /// <param name="id">Identifier of the author to delete.</param>
        /// <param name="cancellationToken">
        /// Token to monitor for cancellation requests.
        /// </param>
        /// <returns>
        /// A <see cref="Task{Boolean}"/> that, when completed,
        /// returns <c>true</c> if the deletion succeeded; <c>false</c> if the author was not found.
        /// </returns>
        Task<bool> DeleteAsync(
            int id,
            CancellationToken cancellationToken = default);
    }
}