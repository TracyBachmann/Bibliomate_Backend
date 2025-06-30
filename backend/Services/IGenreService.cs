using backend.DTOs;

namespace backend.Services
{
    /// <summary>
    /// Defines CRUD operations for Genre entities.
    /// </summary>
    public interface IGenreService
    {
        /// <summary>
        /// Retrieves all genres.
        /// </summary>
        Task<IEnumerable<GenreReadDto>> GetAllAsync();

        /// <summary>
        /// Retrieves a single genre by its identifier.
        /// </summary>
        /// <param name="id">The identifier of the genre to retrieve.</param>
        Task<GenreReadDto?> GetByIdAsync(int id);

        /// <summary>
        /// Creates a new genre.
        /// </summary>
        /// <param name="dto">Data for the genre to create.</param>
        Task<GenreReadDto> CreateAsync(GenreCreateDto dto);

        /// <summary>
        /// Updates an existing genre.
        /// </summary>
        /// <param name="id">The identifier of the genre to update.</param>
        /// <param name="dto">Updated genre data.</param>
        /// <returns>
        /// True if the update succeeded; false if no genre with <paramref name="id"/> exists.
        /// </returns>
        Task<bool> UpdateAsync(int id, GenreCreateDto dto);

        /// <summary>
        /// Deletes a genre by its identifier.
        /// </summary>
        /// <param name="id">The identifier of the genre to delete.</param>
        /// <returns>
        /// True if deletion succeeded; false if no genre with <paramref name="id"/> exists.
        /// </returns>
        Task<bool> DeleteAsync(int id);
    }
}