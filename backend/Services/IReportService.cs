using backend.DTOs;

namespace backend.Services
{
    /// <summary>
    /// Defines operations for generating and managing analytical reports.
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
        /// <returns>
        /// The matching <see cref="ReportReadDto"/>, or <c>null</c> if not found.
        /// </returns>
        Task<ReportReadDto?> GetByIdAsync(int reportId);

        /// <summary>
        /// Generates and persists a new analytical report for the specified user.
        /// </summary>
        /// <param name="dto">
        /// The <see cref="ReportCreateDto"/> containing the <c>Title</c> of the report.
        /// Content is computed automatically from loan and book data.
        /// </param>
        /// <param name="userId">
        /// The identifier of the user who requested the report.
        /// </param>
        /// <returns>The created <see cref="ReportReadDto"/> including computed statistics.</returns>
        Task<ReportReadDto> CreateAsync(ReportCreateDto dto, int userId);

        /// <summary>
        /// Updates an existing report's title and content.
        /// </summary>
        /// <param name="dto">The <see cref="ReportUpdateDto"/> containing updated data.</param>
        /// <returns>
        /// <c>true</c> if the update succeeded; <c>false</c> if the report was not found.
        /// </returns>
        Task<bool> UpdateAsync(ReportUpdateDto dto);

        /// <summary>
        /// Deletes a report by its identifier.
        /// </summary>
        /// <param name="reportId">The identifier of the report to delete.</param>
        /// <returns>
        /// <c>true</c> if the deletion succeeded; <c>false</c> if the report was not found.
        /// </returns>
        Task<bool> DeleteAsync(int reportId);
    }
}
