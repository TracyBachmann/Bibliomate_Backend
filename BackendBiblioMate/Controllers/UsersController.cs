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
    /// Controller for managing application users.
    /// All administration endpoints are restricted to the <c>Admin</c> role,
    /// while authenticated users may access and update their own profile via <c>/me</c> routes.
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
        public UsersController(IUserService service, IUserActivityLogService log)
        {
            _service = service;
            _log     = log;
        }

        /// <summary>
        /// Retrieves all users (admin only).
        /// </summary>
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
        /// Retrieves a user by its identifier (admin only).
        /// </summary>
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
        /// Creates a new user via the admin panel.
        /// </summary>
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
            });
            return CreatedAtAction(nameof(GetUser), new { id = created.UserId }, created);
        }

        /// <summary>
        /// Updates basic info of an existing user (admin only).
        /// Does <b>not</b> allow changing password or role.
        /// </summary>
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
            });
            return NoContent();
        }

        /// <summary>
        /// Updates the currently authenticated user’s personal data.
        /// </summary>
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
            });
            return Ok("Profile updated successfully.");
        }

        /// <summary>
        /// Retrieves the profile of the currently authenticated user.
        /// </summary>
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
        /// Updates a user’s role (admin only).
        /// </summary>
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
            });

            return Ok(new { message = $"Role updated to {dto.Role}." });
        }

        /// <summary>
        /// Deletes a user account (admin only). Cannot delete your own account.
        /// </summary>
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
            });

            return NoContent();
        }
    }
}