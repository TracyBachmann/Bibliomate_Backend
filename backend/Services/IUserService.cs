using backend.DTOs;

namespace backend.Services
{
    /// <summary>
    /// Defines operations for managing application users.
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Retrieves all users.
        /// </summary>
        /// <returns>A collection of <see cref="UserReadDto"/>.</returns>
        Task<IEnumerable<UserReadDto>> GetAllAsync();

        /// <summary>
        /// Retrieves a user by its identifier.
        /// </summary>
        /// <param name="id">The user identifier.</param>
        /// <returns>The <see cref="UserReadDto"/> if found; otherwise null.</returns>
        Task<UserReadDto?> GetByIdAsync(int id);

        /// <summary>
        /// Creates a new user.
        /// </summary>
        /// <param name="dto">The data to create the user.</param>
        /// <returns>The created <see cref="UserReadDto"/>.</returns>
        Task<UserReadDto> CreateAsync(UserCreateDto dto);

        /// <summary>
        /// Updates basic information of an existing user.
        /// Does not change password or role.
        /// </summary>
        /// <param name="id">The user identifier.</param>
        /// <param name="dto">The updated data.</param>
        /// <returns>True if update succeeded; false if user not found.</returns>
        Task<bool> UpdateAsync(int id, UserUpdateDto dto);

        /// <summary>
        /// Updates the role of an existing user.
        /// </summary>
        /// <param name="id">The user identifier.</param>
        /// <param name="dto">The new role data.</param>
        /// <returns>True if update succeeded; false if user not found or invalid role.</returns>
        Task<bool> UpdateRoleAsync(int id, UserRoleUpdateDto dto);

        /// <summary>
        /// Deletes a user.
        /// </summary>
        /// <param name="id">The user identifier.</param>
        /// <returns>True if deletion succeeded; false if user not found or self-deletion attempt.</returns>
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// Retrieves the profile of the current user.
        /// </summary>
        /// <param name="currentUserId">The current user's identifier.</param>
        /// <returns>The <see cref="UserReadDto"/> if found; otherwise null.</returns>
        Task<UserReadDto?> GetCurrentUserAsync(int currentUserId);

        /// <summary>
        /// Updates the profile of the current user.
        /// </summary>
        /// <param name="currentUserId">The current user's identifier.</param>
        /// <param name="dto">The updated profile data.</param>
        /// <returns>True if update succeeded; false if user not found.</returns>
        Task<bool> UpdateCurrentUserAsync(int currentUserId, UserUpdateDto dto);
    }
}