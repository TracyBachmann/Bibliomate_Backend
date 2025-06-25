using backend.DTOs;

namespace backend.Services
{
    /// <summary>
    /// Defines the operations for creating, retrieving, updating
    /// and deleting analytical reports.
    /// </summary>
    public interface IReportService
    {
        /// <summary>
        /// Retrieves all reports, sorted by generation date descending.
        /// </summary>
        /// <returns>A collection of <see cref="ReportReadDto"/>.</returns>
        Task<IEnumerable<ReportReadDto>> GetAllAsync();

        /// <summary>
        /// Retrieves a single report by its identifier.
        /// </summary>
        /// <param name="reportId">The identifier of the report to retrieve.</param>
        /// <returns>The matching <see cref="ReportReadDto"/>, or null if not found.</returns>
        Task<ReportReadDto?> GetByIdAsync(int reportId);

        /// <summary>
        /// Creates a new report for the specified user.
        /// </summary>
        /// <param name="dto">The data to create the report.</param>
        /// <param name="userId">The identifier of the report’s author.</param>
        /// <returns>The created <see cref="ReportReadDto"/>.</returns>
        Task<ReportReadDto> CreateAsync(ReportCreateDto dto, int userId);

        /// <summary>
        /// Updates an existing report.
        /// </summary>
        /// <param name="dto">The updated report data.</param>
        /// <returns>True if the update succeeded; false if not found.</returns>
        Task<bool> UpdateAsync(ReportUpdateDto dto);

        /// <summary>
        /// Deletes a report by its identifier.
        /// </summary>
        /// <param name="reportId">The identifier of the report to delete.</param>
        /// <returns>True if the deletion succeeded; false if not found.</returns>
        Task<bool> DeleteAsync(int reportId);
    }
}