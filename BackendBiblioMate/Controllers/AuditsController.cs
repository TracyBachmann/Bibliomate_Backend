using BackendBiblioMate.Models.Enums;
using BackendBiblioMate.Models.Mongo;
using BackendBiblioMate.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace BackendBiblioMate.Controllers
{
    /// <summary>
    /// Controller for retrieving user activity audit logs.
    /// </summary>
    /// <remarks>
    /// Accessible only by users in the Admin or Librarian roles.
    /// </remarks>
    [ApiController]
    [Route("api/[controller]"), Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Librarian)]
    [Produces("application/json")]
    public class AuditsController : ControllerBase
    {
        private readonly IUserActivityLogService _activityLog;

        /// <summary>
        /// Initializes a new instance of <see cref="AuditsController"/>.
        /// </summary>
        /// <param name="activityLog">
        /// The service used to record and retrieve user activity logs.
        /// </param>
        public AuditsController(IUserActivityLogService activityLog)
        {
            _activityLog = activityLog;
        }

        /// <summary>
        /// Retrieves all activity logs for the specified user.
        /// </summary>
        /// <param name="userId">
        /// The identifier of the user whose logs are requested.
        /// </param>
        /// <param name="cancellationToken">
        /// Token to monitor for cancellation requests.
        /// </param>
        /// <returns>
        /// <c>200 OK</c> with a list of <see cref="UserActivityLogDocument"/> entries;  
        /// <c>404 NotFound</c> if no logs exist for the given user.
        /// </returns>
        /// <response code="200">
        /// Returns the list of activity logs for the user.
        /// </response>
        /// <response code="404">
        /// No activity logs found for the specified user.
        /// </response>
        [HttpGet("user/{userId}/logs")]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Retrieves all activity logs for a user (v1)",
            Description = "Returns the list of activity logs for a specific user.",
            Tags = ["Audits"]
        )]
        [ProducesResponseType(typeof(List<UserActivityLogDocument>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<UserActivityLogDocument>>> GetUserActivityLogs(
            [FromRoute] int userId,
            CancellationToken cancellationToken)
        {
            var logs = await _activityLog.GetByUserAsync(userId, cancellationToken);
            if (logs.Count == 0)
            {
                return NotFound($"No activity logs found for user {userId}.");
            }

            return Ok(logs);
        }
    }
}