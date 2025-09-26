using System.Security.Claims;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BackendBiblioMate.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace BackendBiblioMate.Controllers
{
    /// <summary>
    /// API controller for managing application users.
    /// - Administrative operations (list, create, update others, delete others, change role) are restricted to <c>Admin</c> role.  
    /// - Authenticated users may view and update their own profile via <c>/me</c> endpoints.  
    /// - Provides full CRUD functionality plus role management and self-service profile operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]"), Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _service;
        private readonly IUserActivityLogService _log;

        /// <summary>
        /// Initializes a new instance of <see cref="UsersController"/>.
        /// </summary>
        /// <param name="service">The service handling business logic and persistence for users.</param>
        /// <param name="log">The service responsible for recording user activity logs.</param>
        public UsersController(IUserService service, IUserActivityLogService log)
        {
            _service = service;
            _log     = log;
        }

        /// <summary>
        /// Retrieves all users.
        /// </summary>
        /// <remarks>
        /// - Accessible to <c>Admin</c> only.  
        /// - Returns the complete list of registered users.  
        /// </remarks>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns><c>200 OK</c> with a list of <see cref="UserReadDto"/>.</returns>
        [HttpGet, Authorize(Roles = UserRoles.Admin)]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Retrieves all users (v1)",
            Description = "Admin only endpoint returning all users.",
            Tags = ["Users"]
        )]
        [ProducesResponseType(typeof(IEnumerable<UserReadDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<UserReadDto>>> GetUsers(
            CancellationToken cancellationToken = default)
            => Ok(await _service.GetAllAsync(cancellationToken));

        /// <summary>
        /// Retrieves a user by its identifier.
        /// </summary>
        /// <remarks>
        /// - Accessible to <c>Admin</c> only.  
        /// - Returns <c>404 Not Found</c> if the user does not exist.  
        /// </remarks>
        /// <param name="id">The unique identifier of the user.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>200 OK</c> with the user details.  
        /// <c>404 Not Found</c> if the user does not exist.  
        /// </returns>
        [HttpGet("{id}"), Authorize(Roles = UserRoles.Admin)]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Retrieves a user by ID (v1)",
            Description = "Admin only endpoint returning user details.",
            Tags = ["Users"]
        )]
        [ProducesResponseType(typeof(UserReadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserReadDto>> GetUser(
            [FromRoute] int id,
            CancellationToken cancellationToken = default)
        {
            var dto = await _service.GetByIdAsync(id, cancellationToken);
            if (dto is null) return NotFound();
            return Ok(dto);
        }

        /// <summary>
        /// Creates a new user.
        /// </summary>
        /// <remarks>
        /// - Accessible to <c>Admin</c> only.  
        /// - Logs the creation event in user activity logs.  
        /// </remarks>
        /// <param name="dto">The user data used to create the account.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>201 Created</c> with the created <see cref="UserReadDto"/>.  
        /// <c>400 Bad Request</c> if validation fails.  
        /// </returns>
        [HttpPost, Authorize(Roles = UserRoles.Admin)]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Creates a new user (v1)",
            Description = "Admin only endpoint for creating users.",
            Tags = ["Users"]
        )]
        [ProducesResponseType(typeof(UserReadDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<UserReadDto>> PostUser(
            [FromBody] UserCreateDto dto,
            CancellationToken cancellationToken = default)
        {
            var created = await _service.CreateAsync(dto, cancellationToken);
            await _log.LogAsync(new Models.Mongo.UserActivityLogDocument
            {
                UserId  = created.UserId,
                Action  = "CreateAccount",
                Details = $"Email={created.Email}"
            }, cancellationToken);
            return CreatedAtAction(nameof(GetUser), new { id = created.UserId }, created);
        }

        /// <summary>
        /// Updates basic information of an existing user.
        /// </summary>
        /// <remarks>
        /// - Accessible to <c>Admin</c> only.  
        /// - Does <b>not</b> allow password or role updates.  
        /// - Logs the update in user activity logs.  
        /// </remarks>
        /// <param name="id">The identifier of the user to update.</param>
        /// <param name="dto">The updated user data.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>204 No Content</c> if successfully updated.  
        /// <c>404 Not Found</c> if the user does not exist.  
        /// </returns>
        [HttpPut("{id}"), Authorize(Roles = UserRoles.Admin)]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Updates user info (v1)",
            Description = "Admin only. Does not update password or role.",
            Tags = ["Users"]
        )]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateUser(
            [FromRoute] int id,
            [FromBody] UserUpdateDto dto,
            CancellationToken cancellationToken = default)
        {
            var ok = await _service.UpdateAsync(id, dto, cancellationToken);
            if (!ok) return NotFound();

            await _log.LogAsync(new Models.Mongo.UserActivityLogDocument
            {
                UserId  = id,
                Action  = "UpdateUser",
                Details = $"Updated basic info for user {id}"
            }, cancellationToken);
            return NoContent();
        }

        /// <summary>
        /// Updates the currently authenticated user's profile.
        /// </summary>
        /// <remarks>
        /// - Accessible to any authenticated user.  
        /// - Logs the update action.  
        /// </remarks>
        /// <param name="dto">The updated user data.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>200 OK</c> if successfully updated.  
        /// <c>404 Not Found</c> if the user does not exist.  
        /// </returns>
        [HttpPut("me"), Authorize]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Updates current user's profile (v1)",
            Description = "Authenticated users may update their own data.",
            Tags = ["Users"]
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateCurrentUser(
            [FromBody] UserUpdateDto dto,
            CancellationToken cancellationToken = default)
        {
            int me = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var ok = await _service.UpdateCurrentUserAsync(me, dto, cancellationToken);
            if (!ok) return NotFound();

            await _log.LogAsync(new Models.Mongo.UserActivityLogDocument
            {
                UserId  = me,
                Action  = "UpdateSelf",
                Details = "User updated own profile"
            }, cancellationToken);
            return Ok("Profile updated successfully.");
        }

        /// <summary>
        /// Retrieves the profile of the currently authenticated user.
        /// </summary>
        /// <remarks>
        /// - Accessible to any authenticated user.  
        /// </remarks>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>200 OK</c> with the current user's profile.  
        /// <c>404 Not Found</c> if the user does not exist.  
        /// </returns>
        [HttpGet("me"), Authorize]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Retrieves current user's profile (v1)",
            Description = "Authenticated users may view their own data.",
            Tags = ["Users"]
        )]
        [ProducesResponseType(typeof(UserReadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserReadDto>> GetCurrentUser(
            CancellationToken cancellationToken = default)
        {
            int me = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var dto = await _service.GetCurrentUserAsync(me, cancellationToken);
            if (dto is null) return NotFound();
            return Ok(dto);
        }

        /// <summary>
        /// Updates a user's role.
        /// </summary>
        /// <remarks>
        /// - Accessible to <c>Admin</c> only.  
        /// - Returns <c>400 Bad Request</c> if the role value is invalid.  
        /// - Logs the role change in user activity logs.  
        /// </remarks>
        /// <param name="id">The identifier of the user whose role will be updated.</param>
        /// <param name="dto">The new role information.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>200 OK</c> if successfully updated.  
        /// <c>400 Bad Request</c> if the role is invalid.  
        /// <c>404 Not Found</c> if the user does not exist.  
        /// </returns>
        [HttpPut("{id}/role"), Authorize(Roles = UserRoles.Admin)]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Updates user's role (v1)",
            Description = "Admin only endpoint to change user role.",
            Tags = ["Users"]
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateUserRole(
            [FromRoute] int id,
            [FromBody] UserRoleUpdateDto dto,
            CancellationToken cancellationToken = default)
        {
            var existing = await _service.GetByIdAsync(id, cancellationToken);
            if (existing is null)
                return NotFound();

            var ok = await _service.UpdateRoleAsync(id, dto, cancellationToken);
            if (!ok)
                return BadRequest("Invalid role.");

            await _log.LogAsync(new Models.Mongo.UserActivityLogDocument
            {
                UserId  = id,
                Action  = "UpdateRole",
                Details = $"Role changed to {dto.Role}"
            }, cancellationToken);

            return Ok(new { message = $"Role updated to {dto.Role}." });
        }

        /// <summary>
        /// Deletes a user account.
        /// </summary>
        /// <remarks>
        /// - Accessible to <c>Admin</c> only.  
        /// - An admin cannot delete their own account.  
        /// - Logs the deletion event.  
        /// </remarks>
        /// <param name="id">The identifier of the user to delete.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>204 No Content</c> if successfully deleted.  
        /// <c>400 Bad Request</c> if attempting to delete own account.  
        /// <c>404 Not Found</c> if the user does not exist.  
        /// </returns>
        [HttpDelete("{id}"), Authorize(Roles = UserRoles.Admin)]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Deletes a user (v1)",
            Description = "Admin only endpoint. Cannot delete own account.",
            Tags = ["Users"]
        )]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteUser(
            [FromRoute] int id,
            CancellationToken cancellationToken = default)
        {
            int me = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            if (id == me) return BadRequest("You cannot delete your own account.");

            var ok = await _service.DeleteAsync(id, cancellationToken);
            if (!ok) return NotFound();

            await _log.LogAsync(new Models.Mongo.UserActivityLogDocument
            {
                UserId  = me,
                Action  = "DeleteUser",
                Details = $"Deleted user {id}"
            }, cancellationToken);

            return NoContent();
        }
        
        /// <summary>
        /// Deletes the currently authenticated user's account.
        /// </summary>
        /// <remarks>
        /// - Accessible to any authenticated user.  
        /// - Logs the deletion action.  
        /// </remarks>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns><c>204 No Content</c> if successfully deleted.</returns>
        [HttpDelete("me"), Authorize]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Deletes current user's account (v1)",
            Description = "Authenticated users can delete their own account.",
            Tags = ["Users"]
        )]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteCurrentUser(CancellationToken cancellationToken = default)
        {
            int me = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var ok = await _service.DeleteAsync(me, cancellationToken);
            if (!ok) return NotFound();

            await _log.LogAsync(new Models.Mongo.UserActivityLogDocument
            {
                UserId = me,
                Action = "DeleteSelf",
                Details = "User deleted their own account"
            }, cancellationToken);

            return NoContent();
        }
        
        /// <summary>
        /// Debug endpoint returning the raw claims contained in the current user's authentication token.
        /// </summary>
        /// <remarks>
        /// - Accessible to any authenticated user.  
        /// - Intended for debugging and inspection purposes only.  
        /// </remarks>
        /// <returns>
        /// <c>200 OK</c> with the claims as a collection of key-value pairs.  
        /// </returns>
        [HttpGet("debug-token"), Authorize]
        public IActionResult DebugToken()
        {
            var claims = User.Claims.Select(c => new { c.Type, c.Value });
            return Ok(claims);
        }
    }
}
