using backend.DTOs;

namespace backend.Services
{
    /// <summary>
    /// Defines CRUD operations for Editor entities.
    /// </summary>
    public interface IEditorService
    {
        /// <summary>
        /// Retrieves all editors.
        /// </summary>
        Task<IEnumerable<EditorReadDto>> GetAllAsync();

        /// <summary>
        /// Retrieves a single editor by its identifier.
        /// </summary>
        /// <param name="id">The identifier of the editor to retrieve.</param>
        Task<EditorReadDto?> GetByIdAsync(int id);

        /// <summary>
        /// Creates a new editor.
        /// </summary>
        /// <param name="dto">Data for the editor to create.</param>
        Task<EditorReadDto> CreateAsync(EditorCreateDto dto);

        /// <summary>
        /// Updates an existing editor.
        /// </summary>
        /// <param name="id">The identifier of the editor to update.</param>
        /// <param name="dto">Updated editor data.</param>
        /// <returns>
        /// True if the update succeeded; false if no editor with <paramref name="id"/> exists.
        /// </returns>
        Task<bool> UpdateAsync(int id, EditorCreateDto dto);

        /// <summary>
        /// Deletes an editor by its identifier.
        /// </summary>
        /// <param name="id">The identifier of the editor to delete.</param>
        /// <returns>
        /// True if deletion succeeded; false if no editor with <paramref name="id"/> exists.
        /// </returns>
        Task<bool> DeleteAsync(int id);
    }
}