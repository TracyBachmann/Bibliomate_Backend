using BackendBiblioMate.Models.Enums;
using BackendBiblioMate.Models.Mongo;
using BackendBiblioMate.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace BackendBiblioMate.Controllers
{
    /// <summary>
    /// API controller for retrieving user activity audit logs.
    /// </summary>
    /// <remarks>
    /// All endpoints in this controller are protected and require the caller
    /// to be authenticated with either the <see cref="UserRoles.Admin"/> 
    /// or <see cref="UserRoles.Librarian"/> role.
    /// </remarks>
    [ApiController]
    [Route("api/[controller]")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Librarian)]
    [Produces("application/json")]
    public class AuditsController : ControllerBase
    {
        private readonly IUserActivityLogService _activityLog;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuditsController"/> class.
        /// </summary>
        /// <param name="activityLog">
        /// The service used to query user activity log entries from MongoDB.
        /// </param>
        public AuditsController(IUserActivityLogService activityLog)
        {
            _activityLog = activityLog;
        }

        /// <summary>
        /// Retrieves all recorded activity logs for a specific user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// Returns:
        /// <list type="bullet">
        ///   <item><description><c>200 OK</c> with a list of <see cref="UserActivityLogDocument"/> objects.</description></item>
        ///   <item><description><c>404 NotFound</c> if no activity logs are available for the given user.</description></item>
        /// </list>
        /// </returns>
        /// <response code="200">Activity logs successfully retrieved.</response>
        /// <response code="404">No activity logs found for the specified user.</response>
        [HttpGet("user/{userId}/logs")]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Retrieve all activity logs for a specific user.",
            Description = "Returns the list of user activity logs stored in MongoDB for the provided user identifier.",
            Tags = [ "Audits" ]
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
