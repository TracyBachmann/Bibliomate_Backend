using BackendBiblioMate.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace BackendBiblioMate.Interfaces
{
    /// <summary>
    /// Defines CRUD operations for <see cref="EditorReadDto"/> entities
    /// with detailed service responses.
    /// </summary>
    public interface IEditorService
    {
        /// <summary>
        /// Retrieves all editors.
        /// </summary>
        /// <param name="cancellationToken">
        /// Token to monitor for cancellation requests.
        /// </param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> that, when completed successfully,
        /// yields an <see cref="IEnumerable{EditorReadDto}"/> containing all editors.
        /// </returns>
        Task<IEnumerable<EditorReadDto>> GetAllAsync(
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves an editor by its identifier.
        /// </summary>
        /// <param name="id">The identifier of the editor to retrieve.</param>
        /// <param name="cancellationToken">
        /// Token to monitor for cancellation requests.
        /// </param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> that, when completed, yields a tuple containing:
        /// <list type="bullet">
        ///   <item>
        ///     <description>
        ///       <see cref="EditorReadDto"/> if found; otherwise <c>null</c>.
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
        Task<(EditorReadDto? Dto, IActionResult? ErrorResult)> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new editor.
        /// </summary>
        /// <param name="dto">Data transfer object containing new editor details.</param>
        /// <param name="cancellationToken">
        /// Token to monitor for cancellation requests.
        /// </param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> that, when completed, yields a tuple containing:
        /// <list type="bullet">
        ///   <item>
        ///     <description>The created <see cref="EditorReadDto"/>.</description>
        ///   </item>
        ///   <item>
        ///     <description>
        ///       A <see cref="CreatedAtActionResult"/> with the location header
        ///       pointing to the newly created resource.
        ///     </description>
        ///   </item>
        /// </list>
        /// </returns>
        Task<(EditorReadDto CreatedDto, CreatedAtActionResult Result)> CreateAsync(
            EditorCreateDto dto,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing editor.
        /// </summary>
        /// <param name="id">Identifier of the editor to update.</param>
        /// <param name="dto">Data transfer object with updated editor values.</param>
        /// <param name="cancellationToken">
        /// Token to monitor for cancellation requests.
        /// </param>
        /// <returns>
        /// A <see cref="Task{Boolean}"/> that yields <c>true</c> if the update succeeded;
        /// <c>false</c> if no editor with the given identifier exists.
        /// </returns>
        Task<bool> UpdateAsync(
            int id,
            EditorUpdateDto dto,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes an editor by its identifier.
        /// </summary>
        /// <param name="id">Identifier of the editor to delete.</param>
        /// <param name="cancellationToken">
        /// Token to monitor for cancellation requests.
        /// </param>
        /// <returns>
        /// A <see cref="Task{Boolean}"/> that yields <c>true</c> if the deletion succeeded;
        /// <c>false</c> if no editor with the given identifier exists.
        /// </returns>
        Task<bool> DeleteAsync(
            int id,
            CancellationToken cancellationToken = default);
    }
}