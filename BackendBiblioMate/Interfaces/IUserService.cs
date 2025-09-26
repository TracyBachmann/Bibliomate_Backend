using BackendBiblioMate.DTOs;

namespace BackendBiblioMate.Interfaces
{
    /// <summary>
    /// Defines operations for managing application users, including
    /// CRUD actions, role assignment, and profile management.
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Retrieves all registered users in the system.
        /// </summary>
        /// <param name="cancellationToken">Token to monitor for cancellation requests (optional).</param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> that yields an <see cref="IEnumerable{UserReadDto}"/>
        /// representing all users.
        /// </returns>
        Task<IEnumerable<UserReadDto>> GetAllAsync(
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a single user by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the user.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests (optional).</param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> that yields the corresponding <see cref="UserReadDto"/>,
        /// or <c>null</c> if the user does not exist.
        /// </returns>
        Task<UserReadDto?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new user account.
        /// </summary>
        /// <param name="dto">The <see cref="UserCreateDto"/> containing user data (email, name, role, etc.).</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests (optional).</param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> yielding the created <see cref="UserReadDto"/> entity.
        /// </returns>
        Task<UserReadDto> CreateAsync(
            UserCreateDto dto,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates general information of an existing user (excluding role and password).
        /// </summary>
        /// <param name="id">The unique identifier of the user to update.</param>
        /// <param name="dto">The <see cref="UserUpdateDto"/> containing updated information.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests (optional).</param>
        /// <returns>
        /// <c>true</c> if the update was successful; <c>false</c> if the user was not found.
        /// </returns>
        Task<bool> UpdateAsync(
            int id,
            UserUpdateDto dto,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates the role of an existing user (e.g., User → Librarian, Librarian → Admin).
        /// </summary>
        /// <param name="id">The unique identifier of the user whose role will be changed.</param>
        /// <param name="dto">The <see cref="UserRoleUpdateDto"/> specifying the new role.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests (optional).</param>
        /// <returns>
        /// <c>true</c> if the role update was successful; <c>false</c> if the user was not found.
        /// </returns>
        Task<bool> UpdateRoleAsync(
            int id,
            UserRoleUpdateDto dto,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a user account permanently.
        /// </summary>
        /// <param name="id">The unique identifier of the user to delete.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests (optional).</param>
        /// <returns>
        /// <c>true</c> if deletion succeeded; <c>false</c> if the user was not found.
        /// </returns>
        Task<bool> DeleteAsync(
            int id,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves the profile of the currently authenticated user.
        /// </summary>
        /// <param name="currentUserId">The identifier of the currently authenticated user.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests (optional).</param>
        /// <returns>
        /// A <see cref="UserReadDto"/> representing the profile of the current user,
        /// or <c>null</c> if not found.
        /// </returns>
        Task<UserReadDto?> GetCurrentUserAsync(
            int currentUserId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates the profile of the currently authenticated user (self-service).
        /// </summary>
        /// <param name="currentUserId">The identifier of the current user.</param>
        /// <param name="dto">The <see cref="UserUpdateDto"/> containing updated profile details.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests (optional).</param>
        /// <returns>
        /// <c>true</c> if the profile update was successful; <c>false</c> if the user was not found.
        /// </returns>
        Task<bool> UpdateCurrentUserAsync(
            int currentUserId,
            UserUpdateDto dto,
            CancellationToken cancellationToken = default);
    }
}
