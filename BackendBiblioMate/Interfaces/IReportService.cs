using BackendBiblioMate.DTOs;

namespace BackendBiblioMate.Interfaces
{
    /// <summary>
    /// Defines operations for managing analytical reports, including creation,
    /// retrieval, update, and deletion.
    /// </summary>
    public interface IReportService
    {
        /// <summary>
        /// Retrieves all reports (across all users).
        /// </summary>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="T:System.Threading.Tasks.Task{System.Collections.Generic.List{ReportReadDto}}"/>
        /// containing all reports.
        /// </returns>
        Task<List<ReportReadDto>> GetAllAsync(
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Retrieves all reports for a given user.
        /// </summary>
        /// <param name="userId">The identifier of the user whose reports to retrieve.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> that yields a <see cref="List{ReportReadDto}"/>
        /// containing all reports owned by the user.
        /// </returns>
        Task<List<ReportReadDto>> GetAllForUserAsync(
            int userId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a single report by its identifier.
        /// </summary>
        /// <param name="reportId">The identifier of the report to retrieve.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="Task{ReportReadDto}"/> that yields the matching <see cref="ReportReadDto"/>,
        /// or <c>null</c> if not found.
        /// </returns>
        Task<ReportReadDto?> GetByIdAsync(
            int reportId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Generates and persists a new analytical report for the specified user.
        /// </summary>
        /// <param name="dto">
        /// The <see cref="ReportCreateDto"/> containing the <c>Title</c> of the report.
        /// Content is computed automatically from loan and book data.
        /// </param>
        /// <param name="userId">The identifier of the user who requested the report.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="Task{ReportReadDto}"/> that yields the created <see cref="ReportReadDto"/>
        /// including computed statistics.
        /// </returns>
        Task<ReportReadDto> CreateAsync(
            ReportCreateDto dto,
            int userId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing report's title and content.
        /// </summary>
        /// <param name="dto">The <see cref="ReportUpdateDto"/> containing updated data.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="Task{Boolean}"/> that yields <c>true</c> if the update succeeded;
        /// <c>false</c> if the report was not found.
        /// </returns>
        Task<bool> UpdateAsync(
            ReportUpdateDto dto,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a report by its identifier.
        /// </summary>
        /// <param name="reportId">The identifier of the report to delete.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="Task{Boolean}"/> that yields <c>true</c> if the deletion succeeded;
        /// <c>false</c> if the report was not found.
        /// </returns>
        Task<bool> DeleteAsync(
            int reportId,
            CancellationToken cancellationToken = default);
    }
}