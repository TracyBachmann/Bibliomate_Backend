using BackendBiblioMate.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace BackendBiblioMate.Interfaces
{
    /// <summary>
    /// Defines business logic for managing <see cref="Author"/> entities.
    /// Provides CRUD operations and additional helpers such as search and ensure.
    /// </summary>
    public interface IAuthorService
    {
        /// <summary>
        /// Retrieves all authors in the system.
        /// </summary>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A task producing an <see cref="IEnumerable{T}"/> of <see cref="AuthorReadDto"/>.
        /// </returns>
        Task<IEnumerable<AuthorReadDto>> GetAllAsync(
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a specific author by its identifier.
        /// </summary>
        /// <param name="id">The identifier of the author.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A task producing a tuple:
        /// <list type="bullet">
        ///   <item><description>The <see cref="AuthorReadDto"/> if found, otherwise <c>null</c>.</description></item>
        ///   <item><description>An <see cref="IActionResult"/> if an error occurred (e.g. NotFound), otherwise <c>null</c>.</description></item>
        /// </list>
        /// </returns>
        Task<(AuthorReadDto? Dto, IActionResult? ErrorResult)> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new author record.
        /// </summary>
        /// <param name="dto">The author creation data.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A task producing a tuple:
        /// <list type="bullet">
        ///   <item><description>The created <see cref="AuthorReadDto"/>.</description></item>
        ///   <item><description>A <see cref="CreatedAtActionResult"/> including a Location header pointing to the new resource.</description></item>
        /// </list>
        /// </returns>
        Task<(AuthorReadDto Dto, CreatedAtActionResult Result)> CreateAsync(
            AuthorCreateDto dto,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing author.
        /// </summary>
        /// <param name="id">The identifier of the author to update.</param>
        /// <param name="dto">The updated author data.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A task producing <c>true</c> if the update succeeded, or <c>false</c> if the author was not found.
        /// </returns>
        Task<bool> UpdateAsync(
            int id,
            AuthorCreateDto dto,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes an author by its identifier.
        /// </summary>
        /// <param name="id">The identifier of the author to delete.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A task producing <c>true</c> if the deletion succeeded, or <c>false</c> if the author was not found.
        /// </returns>
        Task<bool> DeleteAsync(
            int id,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Searches authors by a partial name or keyword.
        /// </summary>
        /// <param name="search">The search term (can be <c>null</c> to retrieve all).</param>
        /// <param name="take">The maximum number of results to return.</param>
        /// <param name="ct">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A task producing a filtered <see cref="IEnumerable{AuthorReadDto}"/>.
        /// </returns>
        Task<IEnumerable<AuthorReadDto>> SearchAsync(
            string? search,
            int take,
            CancellationToken ct);

        /// <summary>
        /// Ensures an author with the given name exists.
        /// If the author does not exist, creates it.
        /// </summary>
        /// <param name="name">The name of the author to find or create.</param>
        /// <param name="ct">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A task producing a tuple:
        /// <list type="bullet">
        ///   <item><description>The matching or newly created <see cref="AuthorReadDto"/>.</description></item>
        ///   <item><description><c>true</c> if the author was created, <c>false</c> if it already existed.</description></item>
        /// </list>
        /// </returns>
        Task<(AuthorReadDto Dto, bool Created)> EnsureAsync(
            string name,
            CancellationToken ct);
    }
}
