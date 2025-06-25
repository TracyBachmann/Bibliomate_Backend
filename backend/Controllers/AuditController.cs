using backend.Models.Mongo;
using backend.Models.Enums;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    /// <summary>
    /// Controller for retrieving user activity audit logs.
    /// Accessible only by users in the Admin or Librarian roles.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Librarian}")]
    public class AuditController : ControllerBase
    {
        private readonly IUserActivityLogService _activityLog;

        /// <summary>
        /// Initializes a new instance of <see cref="AuditController"/>.
        /// </summary>
        /// <param name="activityLog">
        /// The service used to record and retrieve user activity logs.
        /// </param>
        public AuditController(IUserActivityLogService activityLog)
        {
            _activityLog = activityLog;
        }

        /// <summary>
        /// Retrieves all activity logs for the specified user.
        /// </summary>
        /// <param name="userId">The identifier of the user whose logs are requested.</param>
        /// <returns>
        /// <c>200 OK</c> with a list of <see cref="UserActivityLogDocument"/> entries;  
        /// <c>404 NotFound</c> if no logs exist for the given user.
        /// </returns>
        [HttpGet("user/{userId}/logs")]
        public async Task<ActionResult<List<UserActivityLogDocument>>> GetUserActivityLogs(int userId)
        {
            var logs = await _activityLog.GetByUserAsync(userId);

            if (logs == null || logs.Count == 0)
                return NotFound($"No activity logs found for user {userId}.");

            return Ok(logs);
        }
    }
}