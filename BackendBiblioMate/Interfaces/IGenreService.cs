using BackendBiblioMate.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace BackendBiblioMate.Interfaces
{
    /// <summary>
    /// Defines CRUD operations for <see cref="GenreReadDto"/> entities
    /// with detailed service responses.
    /// </summary>
    public interface IGenreService
    {
        /// <summary>
        /// Retrieves all genres.
        /// </summary>
        /// <param name="cancellationToken">
        /// Token to monitor for cancellation requests.
        /// </param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> that, when completed successfully,
        /// yields an <see cref="IEnumerable{GenreReadDto}"/> containing all genres.
        /// </returns>
        Task<IEnumerable<GenreReadDto>> GetAllAsync(
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a genre by its identifier.
        /// </summary>
        /// <param name="id">Identifier of the genre to retrieve.</param>
        /// <param name="cancellationToken">
        /// Token to monitor for cancellation requests.
        /// </param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> that, when completed, yields a tuple containing:
        /// <list type="bullet">
        ///   <item>
        ///     <description>
        ///       <see cref="GenreReadDto"/> if found; otherwise <c>null</c>.
        ///     </description>
        ///   </item>
        ///   <item>
        ///     <description>
        ///       An <see cref="IActionResult"/> representing the error
        ///       response (e.g., <c>NotFound</c>), or <c>null</c> on success.
        ///     </description>
        ///   </item>
        /// </list>
        /// </returns>
        Task<(GenreReadDto? Dto, IActionResult? ErrorResult)> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new genre.
        /// </summary>
        /// <param name="dto">Data transfer object containing new genre details.</param>
        /// <param name="cancellationToken">
        /// Token to monitor for cancellation requests.
        /// </param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> that, when completed, yields a tuple containing:
        /// <list type="bullet">
        ///   <item>
        ///     <description>The created <see cref="GenreReadDto"/>.</description>
        ///   </item>
        ///   <item>
        ///     <description>
        ///       A <see cref="CreatedAtActionResult"/> with the Location
        ///       header pointing to the newly created resource.
        ///     </description>
        ///   </item>
        /// </list>
        /// </returns>
        Task<(GenreReadDto CreatedDto, CreatedAtActionResult Result)> CreateAsync(
            GenreCreateDto dto,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing genre.
        /// </summary>
        /// <param name="id">Identifier of the genre to update.</param>
        /// <param name="dto">Data transfer object with updated genre values.</param>
        /// <param name="cancellationToken">
        /// Token to monitor for cancellation requests.
        /// </param>
        /// <returns>
        /// A <see cref="Task{Boolean}"/> that yields <c>true</c> if the update succeeded;
        /// <c>false</c> if no genre with the given identifier exists.
        /// </returns>
        Task<bool> UpdateAsync(
            int id,
            GenreUpdateDto dto,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a genre by its identifier.
        /// </summary>
        /// <param name="id">Identifier of the genre to delete.</param>
        /// <param name="cancellationToken">
        /// Token to monitor for cancellation requests.
        /// </param>
        /// <returns>
        /// A <see cref="Task{Boolean}"/> that yields <c>true</c> if the deletion succeeded;
        /// <c>false</c> if no genre with the given identifier exists.
        /// </returns>
        Task<bool> DeleteAsync(
            int id,
            CancellationToken cancellationToken = default);
    }
}