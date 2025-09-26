using BackendBiblioMate.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace BackendBiblioMate.Interfaces
{
    /// <summary>
    /// Defines CRUD operations for <see cref="GenreReadDto"/> entities,
    /// including search and ensure functionality for maintaining genre consistency.
    /// </summary>
    public interface IGenreService
    {
        /// <summary>
        /// Retrieves all genres.
        /// </summary>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A collection of <see cref="GenreReadDto"/> items representing all genres.
        /// </returns>
        Task<IEnumerable<GenreReadDto>> GetAllAsync(
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a genre by its identifier.
        /// </summary>
        /// <param name="id">The identifier of the genre.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A tuple containing:
        /// <list type="bullet">
        ///   <item><description><see cref="GenreReadDto"/> if found; otherwise <c>null</c>.</description></item>
        ///   <item><description>An <see cref="IActionResult"/> representing an error response (e.g. <c>NotFound</c>), or <c>null</c> on success.</description></item>
        /// </list>
        /// </returns>
        Task<(GenreReadDto? Dto, IActionResult? ErrorResult)> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new genre.
        /// </summary>
        /// <param name="dto">The DTO containing the genre’s creation data.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A tuple containing:
        /// <list type="bullet">
        ///   <item><description>The created <see cref="GenreReadDto"/>.</description></item>
        ///   <item><description>A <see cref="CreatedAtActionResult"/> with the location header pointing to the new resource.</description></item>
        /// </list>
        /// </returns>
        Task<(GenreReadDto CreatedDto, CreatedAtActionResult Result)> CreateAsync(
            GenreCreateDto dto,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing genre.
        /// </summary>
        /// <param name="id">The identifier of the genre to update.</param>
        /// <param name="dto">The DTO containing updated values.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>true</c> if the update succeeded; <c>false</c> if the genre does not exist.
        /// </returns>
        Task<bool> UpdateAsync(
            int id,
            GenreUpdateDto dto,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a genre by its identifier.
        /// </summary>
        /// <param name="id">The identifier of the genre to delete.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>true</c> if the deletion succeeded; <c>false</c> if the genre does not exist.
        /// </returns>
        Task<bool> DeleteAsync(
            int id,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Performs a search for genres by name.
        /// </summary>
        /// <param name="search">Optional search string to filter by name.</param>
        /// <param name="take">Maximum number of results to return.</param>
        /// <param name="ct">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A collection of matching <see cref="GenreReadDto"/> items.
        /// </returns>
        Task<IEnumerable<GenreReadDto>> SearchAsync(string? search, int take, CancellationToken ct);

        /// <summary>
        /// Ensures that a genre with the given name exists.
        /// Creates it if it does not already exist.
        /// </summary>
        /// <param name="name">The name of the genre to check or create.</param>
        /// <param name="ct">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A tuple containing:
        /// <list type="bullet">
        ///   <item><description>The existing or newly created <see cref="GenreReadDto"/>.</description></item>
        ///   <item><description><c>true</c> if the genre was created; <c>false</c> if it already existed.</description></item>
        /// </list>
        /// </returns>
        Task<(GenreReadDto Dto, bool Created)> EnsureAsync(string name, CancellationToken ct);
    }
}
