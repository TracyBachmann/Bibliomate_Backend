using BackendBiblioMate.DTOs;

namespace BackendBiblioMate.Interfaces
{
    /// <summary>
    /// Defines operations for managing book reservations,
    /// including creation, retrieval, update, and deletion.
    /// Enforces ownership rules and supports librarian/admin access.
    /// </summary>
    public interface IReservationService
    {
        /// <summary>
        /// Retrieves all reservations in the system.
        /// Intended for use by Librarians and Admins only.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> used to observe cancellation requests.
        /// </param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> that, when completed, yields an
        /// <see cref="IEnumerable{ReservationReadDto}"/> containing all reservations.
        /// </returns>
        Task<IEnumerable<ReservationReadDto>> GetAllAsync(
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves all active reservations for a given user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> used to observe cancellation requests.
        /// </param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> that yields an
        /// <see cref="IEnumerable{ReservationReadDto}"/> containing the user's active reservations.
        /// </returns>
        Task<IEnumerable<ReservationReadDto>> GetByUserAsync(
            int userId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves all pending reservations for a specific book.
        /// Pending reservations are those waiting to be fulfilled when a copy becomes available.
        /// </summary>
        /// <param name="bookId">The unique identifier of the book.</param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> used to observe cancellation requests.
        /// </param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> that yields an
        /// <see cref="IEnumerable{ReservationReadDto}"/> containing pending reservations for the book.
        /// </returns>
        Task<IEnumerable<ReservationReadDto>> GetPendingForBookAsync(
            int bookId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a reservation by its identifier.
        /// </summary>
        /// <param name="reservationId">The unique identifier of the reservation.</param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> used to observe cancellation requests.
        /// </param>
        /// <returns>
        /// A <see cref="Task{ReservationReadDto}"/> that yields the matching
        /// <see cref="ReservationReadDto"/> if found; otherwise <c>null</c>.
        /// </returns>
        Task<ReservationReadDto?> GetByIdAsync(
            int reservationId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new reservation for a user.
        /// </summary>
        /// <param name="dto">The <see cref="ReservationCreateDto"/> containing reservation details.</param>
        /// <param name="userId">The identifier of the user creating the reservation.</param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> used to observe cancellation requests.
        /// </param>
        /// <returns>
        /// A <see cref="Task{ReservationReadDto}"/> that yields the created
        /// <see cref="ReservationReadDto"/> with its identifier and metadata.
        /// </returns>
        Task<ReservationReadDto> CreateAsync(
            ReservationCreateDto dto,
            int userId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing reservation.
        /// </summary>
        /// <param name="dto">The <see cref="ReservationUpdateDto"/> containing updated reservation data.</param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> used to observe cancellation requests.
        /// </param>
        /// <returns>
        /// A <see cref="Task{Boolean}"/> that yields <c>true</c> if the update succeeded;
        /// <c>false</c> if no reservation with the given identifier exists.
        /// </returns>
        Task<bool> UpdateAsync(
            ReservationUpdateDto dto,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a reservation by its identifier.
        /// </summary>
        /// <param name="reservationId">The unique identifier of the reservation to delete.</param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> used to observe cancellation requests.
        /// </param>
        /// <returns>
        /// A <see cref="Task{Boolean}"/> that yields <c>true</c> if the deletion succeeded;
        /// <c>false</c> if no reservation with the given identifier exists.
        /// </returns>
        Task<bool> DeleteAsync(
            int reservationId,
            CancellationToken cancellationToken = default);
    }
}
