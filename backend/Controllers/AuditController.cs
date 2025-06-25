using backend.Models.Mongo;
using backend.Services;
using backend.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    /// <summary>
    /// Controller for retrieving audit logs of user activities.
    /// Only accessible to Librarians and Admins.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Librarian}")]
    public class AuditController : ControllerBase
    {
        private readonly UserActivityLogService _activityLog;

        /// <summary>
        /// Initializes a new instance of <see cref="AuditController"/>.
        /// </summary>
        /// <param name="activityLog">
        /// Service for retrieving and logging user activity events.
        /// </param>
        public AuditController(UserActivityLogService activityLog)
        {
            _activityLog = activityLog;
        }

        /// <summary>
        /// Retrieves all activity logs for a specific user.
        /// </summary>
        /// <param name="userId">The identifier of the user whose logs are requested.</param>
        /// <returns>
        /// <c>200 OK</c> with a list of <see cref="UserActivityLogDocument"/> entries; 
        /// <c>404 NotFound</c> if no logs exist for the given user.
        /// </returns>
        [HttpGet("user/{userId}/logs")]
        public async Task<IActionResult> GetUserActivityLogs(int userId)
        {
            List<UserActivityLogDocument> logs = await _activityLog.GetByUserAsync(userId);

            if (logs == null || logs.Count == 0)
                return NotFound($"No activity logs found for user {userId}.");

            return Ok(logs);
        }
    }
}