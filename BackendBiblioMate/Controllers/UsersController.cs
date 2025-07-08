using System.Security.Claims;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BackendBiblioMate.Interfaces;

namespace BackendBiblioMate.Controllers
{
    /// <summary>
    /// Controller for managing application users.
    /// All administration endpoints are restricted to the <c>Admin</c> role,
    /// while authenticated users may access and update their own profile via <c>/me</c> routes.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _service;
        private readonly IUserActivityLogService _log;

        /// <summary>
        /// Initializes a new instance of <see cref="UsersController"/>.
        /// </summary>
        /// <param name="service">Service encapsulating user logic.</param>
        /// <param name="log">Service for logging user activities.</param>
        public UsersController(IUserService service, IUserActivityLogService log)
        {
            _service = service;
            _log     = log;
        }

        /// <summary>
        /// Retrieves all users (admin only).
        /// </summary>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>200 OK</c> with a collection of <see cref="UserReadDto"/>.
        /// </returns>
        [HttpGet, Authorize(Roles = UserRoles.Admin)]
        [ProducesResponseType(typeof(IEnumerable<UserReadDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<UserReadDto>>> GetUsers(
            CancellationToken cancellationToken = default)
            => Ok(await _service.GetAllAsync(cancellationToken));

        /// <summary>
        /// Retrieves a user by its identifier (admin only).
        /// </summary>
        /// <param name="id">The user identifier.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>200 OK</c> with <see cref="UserReadDto"/> if found;  
        /// <c>404 NotFound</c> otherwise.
        /// </returns>
        [HttpGet("{id}"), Authorize(Roles = UserRoles.Admin)]
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
        /// Creates a new user via the admin panel.
        /// </summary>
        /// <param name="dto">User registration data.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>201 Created</c> with the created <see cref="UserReadDto"/>;  
        /// <c>400 BadRequest</c> if validation fails.
        /// </returns>
        [HttpPost, Authorize(Roles = UserRoles.Admin)]
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
            });
            return CreatedAtAction(nameof(GetUser), new { id = created.UserId }, created);
        }

        /// <summary>
        /// Updates basic info of an existing user (admin only).
        /// Does <b>not</b> allow changing password or role.
        /// </summary>
        /// <param name="id">The user identifier.</param>
        /// <param name="dto">Updated data.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>204 NoContent</c> on success;  
        /// <c>404 NotFound</c> if user not found.
        /// </returns>
        [HttpPut("{id}"), Authorize(Roles = UserRoles.Admin)]
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
            });
            return NoContent();
        }

        /// <summary>
        /// Updates the currently authenticated user’s personal data.
        /// </summary>
        /// <param name="dto">The updated profile data.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>200 OK</c> on success;  
        /// <c>404 NotFound</c> if user not found.
        /// </returns>
        [HttpPut("me"), Authorize]
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
            });
            return Ok("Profile updated successfully.");
        }

        /// <summary>
        /// Retrieves the profile of the currently authenticated user.
        /// </summary>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>200 OK</c> with the user’s profile data;  
        /// <c>404 NotFound</c> if user not found.
        /// </returns>
        [HttpGet("me"), Authorize]
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
        /// Updates a user’s role (admin only).
        /// </summary>
        /// <param name="id">The user identifier.</param>
        /// <param name="dto">New role data.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>200 OK</c> on success;  
        /// <c>400 BadRequest</c> for invalid role;  
        /// <c>404 NotFound</c> if user not found.
        /// </returns>
        [HttpPut("{id}/role"), Authorize(Roles = UserRoles.Admin)]
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
            });

            return Ok(new { message = $"Role updated to {dto.Role}." });
        }

        /// <summary>
        /// Deletes a user account (admin only). Cannot delete your own account.
        /// </summary>
        /// <param name="id">The user identifier.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>204 NoContent</c> on success;  
        /// <c>400 BadRequest</c> if self-deletion attempt;  
        /// <c>404 NotFound</c> if user not found.
        /// </returns>
        [HttpDelete("{id}"), Authorize(Roles = UserRoles.Admin)]
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
            });

            return NoContent();
        }
    }
}