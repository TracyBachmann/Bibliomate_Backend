using BackendBiblioMate.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace BackendBiblioMate.Interfaces
{
    /// <summary>
    /// Defines CRUD and utility operations for <see cref="EditorReadDto"/> entities,
    /// providing consistent service responses.
    /// </summary>
    public interface IEditorService
    {
        /// <summary>
        /// Retrieves all editors.
        /// </summary>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A task yielding all <see cref="EditorReadDto"/> items.
        /// </returns>
        Task<IEnumerable<EditorReadDto>> GetAllAsync(
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves an editor by its identifier.
        /// </summary>
        /// <param name="id">The identifier of the editor to retrieve.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A task yielding a tuple:
        /// <list type="bullet">
        ///   <item><description>The <see cref="EditorReadDto"/> if found; otherwise <c>null</c>.</description></item>
        ///   <item><description>An <see cref="IActionResult"/> error response (e.g. <c>NotFound</c>), or <c>null</c> if successful.</description></item>
        /// </list>
        /// </returns>
        Task<(EditorReadDto? Dto, IActionResult? ErrorResult)> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new editor.
        /// </summary>
        /// <param name="dto">Data transfer object containing new editor details.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A task yielding a tuple:
        /// <list type="bullet">
        ///   <item><description>The created <see cref="EditorReadDto"/>.</description></item>
        ///   <item><description>A <see cref="CreatedAtActionResult"/> pointing to the new resource.</description></item>
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
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A task yielding <c>true</c> if the update succeeded; <c>false</c> if the editor was not found.
        /// </returns>
        Task<bool> UpdateAsync(
            int id,
            EditorUpdateDto dto,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes an editor by its identifier.
        /// </summary>
        /// <param name="id">Identifier of the editor to delete.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A task yielding <c>true</c> if the deletion succeeded; <c>false</c> if not found.
        /// </returns>
        Task<bool> DeleteAsync(
            int id,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Searches editors by name.
        /// </summary>
        /// <param name="search">Optional search term to match editor names.</param>
        /// <param name="take">Maximum number of results to return.</param>
        /// <param name="ct">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A task yielding the matching <see cref="EditorReadDto"/> items.
        /// </returns>
        Task<IEnumerable<EditorReadDto>> SearchAsync(
            string? search,
            int take,
            CancellationToken ct);

        /// <summary>
        /// Ensures that an editor exists with the given name.
        /// If the editor does not exist, creates it.
        /// </summary>
        /// <param name="name">The name of the editor to ensure.</param>
        /// <param name="ct">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A task yielding a tuple:
        /// <list type="bullet">
        ///   <item><description>The existing or newly created <see cref="EditorReadDto"/>.</description></item>
        ///   <item><description><c>true</c> if the editor was created; <c>false</c> if it already existed.</description></item>
        /// </list>
        /// </returns>
        Task<(EditorReadDto Dto, bool Created)> EnsureAsync(
            string name,
            CancellationToken ct);
    }
}
