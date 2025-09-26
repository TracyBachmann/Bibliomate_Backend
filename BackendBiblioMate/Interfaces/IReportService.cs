using BackendBiblioMate.DTOs;

namespace BackendBiblioMate.Interfaces
{
    /// <summary>
    /// Defines operations for managing analytical reports, including
    /// creation, retrieval, update, and deletion.
    /// Reports typically contain computed statistics derived from loans,
    /// books, and other related entities.
    /// </summary>
    public interface IReportService
    {
        /// <summary>
        /// Retrieves all reports across all users.
        /// Intended primarily for administrative or librarian use.
        /// </summary>
        /// <param name="cancellationToken">
        /// A token to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> that yields a
        /// <see cref="List{ReportReadDto}"/> containing all reports.
        /// The list may be empty if no reports exist.
        /// </returns>
        Task<List<ReportReadDto>> GetAllAsync(
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves all reports created by a specific user.
        /// </summary>
        /// <param name="userId">The identifier of the user whose reports are requested.</param>
        /// <param name="cancellationToken">
        /// A token to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> yielding a
        /// <see cref="List{ReportReadDto}"/> containing all reports
        /// owned by the specified user. The list may be empty if the
        /// user has not created any reports.
        /// </returns>
        Task<List<ReportReadDto>> GetAllForUserAsync(
            int userId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a single report by its unique identifier.
        /// </summary>
        /// <param name="reportId">The identifier of the report to retrieve.</param>
        /// <param name="cancellationToken">
        /// A token to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> yielding the matching
        /// <see cref="ReportReadDto"/>, or <c>null</c> if no report
        /// with the specified identifier exists.
        /// </returns>
        Task<ReportReadDto?> GetByIdAsync(
            int reportId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Generates and persists a new analytical report for the specified user.
        /// The report content is automatically computed from loan and book data.
        /// </summary>
        /// <param name="dto">
        /// The <see cref="ReportCreateDto"/> containing metadata such as the report <c>Title</c>.
        /// </param>
        /// <param name="userId">The identifier of the user requesting the report.</param>
        /// <param name="cancellationToken">
        /// A token to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> yielding the created <see cref="ReportReadDto"/>,
        /// including the computed statistics and metadata.
        /// </returns>
        Task<ReportReadDto> CreateAsync(
            ReportCreateDto dto,
            int userId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing report's title and/or content.
        /// </summary>
        /// <param name="dto">
        /// The <see cref="ReportUpdateDto"/> containing updated values.
        /// </param>
        /// <param name="cancellationToken">
        /// A token to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A <see cref="Task{Boolean}"/> that yields <c>true</c> if the update succeeded;
        /// <c>false</c> if no report with the specified identifier was found.
        /// </returns>
        Task<bool> UpdateAsync(
            ReportUpdateDto dto,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Permanently deletes a report by its identifier.
        /// </summary>
        /// <param name="reportId">The identifier of the report to delete.</param>
        /// <param name="cancellationToken">
        /// A token to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A <see cref="Task{Boolean}"/> that yields <c>true</c> if the report was deleted;
        /// <c>false</c> if the report was not found.
        /// </returns>
        Task<bool> DeleteAsync(
            int reportId,
            CancellationToken cancellationToken = default);
    }
}
