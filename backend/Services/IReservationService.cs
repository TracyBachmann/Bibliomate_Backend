using backend.DTOs;

namespace backend.Services
{
    /// <summary>
    /// Defines operations for managing book reservations.
    /// </summary>
    public interface IReservationService
    {
        /// <summary>
        /// Retrieves all reservations (for librarians and admins).
        /// </summary>
        Task<IEnumerable<ReservationReadDto>> GetAllAsync();

        /// <summary>
        /// Retrieves active reservations for a given user.
        /// </summary>
        /// <param name="userId">The user’s identifier.</param>
        Task<IEnumerable<ReservationReadDto>> GetByUserAsync(int userId);

        /// <summary>
        /// Retrieves pending reservations for a given book.
        /// </summary>
        /// <param name="bookId">The book’s identifier.</param>
        Task<IEnumerable<ReservationReadDto>> GetPendingForBookAsync(int bookId);

        /// <summary>
        /// Retrieves a single reservation by its identifier.
        /// </summary>
        /// <param name="reservationId">The reservation identifier.</param>
        Task<ReservationReadDto?> GetByIdAsync(int reservationId);

        /// <summary>
        /// Creates a new reservation.
        /// </summary>
        /// <param name="dto">The creation data.</param>
        /// <param name="userId">The identifier of the reserving user.</param>
        Task<ReservationReadDto> CreateAsync(ReservationCreateDto dto, int userId);

        /// <summary>
        /// Updates an existing reservation’s data.
        /// </summary>
        /// <param name="dto">The updated reservation data.</param>
        /// <returns>True if updated; false if not found.</returns>
        Task<bool> UpdateAsync(ReservationUpdateDto dto);

        /// <summary>
        /// Deletes a reservation by its identifier.
        /// </summary>
        /// <param name="reservationId">The reservation identifier.</param>
        /// <returns>True if deleted; false if not found.</returns>
        Task<bool> DeleteAsync(int reservationId);
    }
}