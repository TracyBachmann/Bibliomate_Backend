using BackendBiblioMate.DTOs;

namespace BackendBiblioMate.Interfaces
{
    /// <summary>
    /// Defines operations for managing book reservations.
    /// </summary>
    public interface IReservationService
    {
        /// <summary>
        /// Retrieves all reservations (for librarians and admins).
        /// </summary>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="Task{IEnumerable}"/> that yields all <see cref="ReservationReadDto"/>.
        /// </returns>
        Task<IEnumerable<ReservationReadDto>> GetAllAsync(
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves active reservations for a given user.
        /// </summary>
        /// <param name="userId">The user’s identifier.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="Task{IEnumerable}"/> that yields the user’s active <see cref="ReservationReadDto"/>.
        /// </returns>
        Task<IEnumerable<ReservationReadDto>> GetByUserAsync(
            int userId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves pending reservations for a given book.
        /// </summary>
        /// <param name="bookId">The book’s identifier.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="Task{IEnumerable}"/> that yields pending <see cref="ReservationReadDto"/> for the book.
        /// </returns>
        Task<IEnumerable<ReservationReadDto>> GetPendingForBookAsync(
            int bookId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a single reservation by its identifier.
        /// </summary>
        /// <param name="reservationId">The reservation identifier.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="Task{ReservationReadDto}"/> that yields the matching <see cref="ReservationReadDto"/>,
        /// or <c>null</c> if not found.
        /// </returns>
        Task<ReservationReadDto?> GetByIdAsync(
            int reservationId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new reservation.
        /// </summary>
        /// <param name="dto">The creation data transfer object.</param>
        /// <param name="userId">The identifier of the reserving user.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="Task{ReservationReadDto}"/> that yields the created <see cref="ReservationReadDto"/>.
        /// </returns>
        Task<ReservationReadDto> CreateAsync(
            ReservationCreateDto dto,
            int userId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing reservation’s data.
        /// </summary>
        /// <param name="dto">The updated reservation data transfer object.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
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
        /// <param name="reservationId">The reservation identifier.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="Task{Boolean}"/> that yields <c>true</c> if the deletion succeeded;
        /// <c>false</c> if no reservation with the given identifier exists.
        /// </returns>
        Task<bool> DeleteAsync(
            int reservationId,
            CancellationToken cancellationToken = default);
    }
}