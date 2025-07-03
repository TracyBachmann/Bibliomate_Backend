using BackendBiblioMate.DTOs;

namespace BackendBiblioMate.Interfaces
{
    /// <summary>
    /// Defines operations for managing application users.
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Retrieves all users.
        /// </summary>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="T:Task{IEnumerable{UserReadDto}}"/>
        /// that yields all <see cref="T:UserReadDto"/> items.
        /// </returns>
        Task<IEnumerable<UserReadDto>> GetAllAsync(
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a user by its identifier.
        /// </summary>
        /// <param name="id">The user identifier.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="Task{UserReadDto}"/> that yields the matching user, or <c>null</c> if not found.
        /// </returns>
        Task<UserReadDto?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new user.
        /// </summary>
        /// <param name="dto">The data to create the user.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="Task{UserReadDto}"/> that yields the created user.
        /// </returns>
        Task<UserReadDto> CreateAsync(
            UserCreateDto dto,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates basic information of an existing user (excluding password and role).
        /// </summary>
        /// <param name="id">The user identifier.</param>
        /// <param name="dto">The updated data.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="Task{Boolean}"/> that yields <c>true</c> if update succeeded; <c>false</c> if not found.
        /// </returns>
        Task<bool> UpdateAsync(
            int id,
            UserUpdateDto dto,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates the role of an existing user.
        /// </summary>
        /// <param name="id">The user identifier.</param>
        /// <param name="dto">The new role data.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="Task{Boolean}"/> that yields <c>true</c> if role update succeeded; <c>false</c> if not found.
        /// </returns>
        Task<bool> UpdateRoleAsync(
            int id,
            UserRoleUpdateDto dto,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a user.
        /// </summary>
        /// <param name="id">The user identifier.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="Task{Boolean}"/> that yields <c>true</c> if deletion succeeded; <c>false</c> if not found.
        /// </returns>
        Task<bool> DeleteAsync(
            int id,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves the profile of the current user.
        /// </summary>
        /// <param name="currentUserId">The current user's identifier.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="Task{UserReadDto}"/> that yields the current user's profile, or <c>null</c> if not found.
        /// </returns>
        Task<UserReadDto?> GetCurrentUserAsync(
            int currentUserId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates the profile of the current user.
        /// </summary>
        /// <param name="currentUserId">The current user's identifier.</param>
        /// <param name="dto">The updated profile data.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="Task{Boolean}"/> that yields <c>true</c> if profile update succeeded; <c>false</c> if not found.
        /// </returns>
        Task<bool> UpdateCurrentUserAsync(
            int currentUserId,
            UserUpdateDto dto,
            CancellationToken cancellationToken = default);
    }
}