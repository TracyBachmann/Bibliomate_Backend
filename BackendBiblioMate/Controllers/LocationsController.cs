using BackendBiblioMate.DTOs;
using BackendBiblioMate.Interfaces;
using BackendBiblioMate.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace BackendBiblioMate.Controllers
{
    /// <summary>
    /// Controller for managing library locations such as floors, aisles, shelves, and levels.
    /// Provides endpoints for retrieving and ensuring complete location hierarchies.
    /// </summary>
    [ApiController]
    [Route("api/[controller]"), Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    public class LocationsController : ControllerBase
    {
        private readonly ILocationService _svc;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocationsController"/>.
        /// </summary>
        /// <param name="svc">The location service handling location logic.</param>
        public LocationsController(ILocationService svc) => _svc = svc;

        /// <summary>
        /// Retrieves all floors.
        /// </summary>
        /// <param name="ct">Token to monitor for cancellation requests.</param>
        /// <returns>A list of <see cref="FloorReadDto"/>.</returns>
        [HttpGet("floors"), AllowAnonymous]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "List floors (v1)",
            Description = "Returns all floors available in the library.",
            Tags = ["Locations"]
        )]
        [ProducesResponseType(typeof(IEnumerable<FloorReadDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<FloorReadDto>>> GetFloors(CancellationToken ct)
            => Ok(await _svc.GetFloorsAsync(ct));

        /// <summary>
        /// Retrieves all aisles for a given floor.
        /// </summary>
        /// <param name="floor">The floor number.</param>
        /// <param name="ct">Token to monitor for cancellation requests.</param>
        /// <returns>A list of <see cref="AisleReadDto"/>.</returns>
        [HttpGet("aisles"), AllowAnonymous]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "List aisles for a floor (v1)",
            Description = "Returns all aisles located on the given floor.",
            Tags = ["Locations"]
        )]
        [ProducesResponseType(typeof(IEnumerable<AisleReadDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<AisleReadDto>>> GetAisles(
            [FromQuery] int floor,
            CancellationToken ct)
        {
            var aisles = await _svc.GetAislesAsync(floor, ct);
            if (!aisles.Any()) return NotFound();
            return Ok(aisles);
        }

        /// <summary>
        /// Retrieves all shelves for a given floor and aisle.
        /// </summary>
        /// <param name="floor">The floor number.</param>
        /// <param name="aisle">The aisle code or name.</param>
        /// <param name="ct">Token to monitor for cancellation requests.</param>
        /// <returns>A list of <see cref="ShelfMiniReadDto"/>.</returns>
        [HttpGet("shelves"), AllowAnonymous]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "List shelves (rayons) for a floor and aisle (v1)",
            Description = "Returns all shelves for the given floor and aisle.",
            Tags = ["Locations"]
        )]
        [ProducesResponseType(typeof(IEnumerable<ShelfMiniReadDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<ShelfMiniReadDto>>> GetShelves(
            [FromQuery] int floor,
            [FromQuery] string aisle,
            CancellationToken ct)
        {
            var shelves = await _svc.GetShelvesAsync(floor, aisle, ct);
            if (!shelves.Any()) return NotFound();
            return Ok(shelves);
        }

        /// <summary>
        /// Retrieves all levels for a given shelf.
        /// </summary>
        /// <param name="shelfId">The shelf identifier.</param>
        /// <param name="ct">Token to monitor for cancellation requests.</param>
        /// <returns>A list of <see cref="LevelReadDto"/>.</returns>
        [HttpGet("levels"), AllowAnonymous]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "List levels for a shelf (v1)",
            Description = "Returns all levels for the given shelf.",
            Tags = ["Locations"]
        )]
        [ProducesResponseType(typeof(IEnumerable<LevelReadDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<LevelReadDto>>> GetLevels(
            [FromQuery] int shelfId,
            CancellationToken ct)
        {
            var levels = await _svc.GetLevelsAsync(shelfId, ct);
            if (!levels.Any()) return NotFound();
            return Ok(levels);
        }

        /// <summary>
        /// Ensures a complete location exists (floor, aisle, shelf, and level).
        /// If missing, creates it atomically.
        /// </summary>
        /// <param name="dto">The location details.</param>
        /// <param name="ct">Token to monitor for cancellation requests.</param>
        /// <returns>The ensured <see cref="LocationReadDto"/>.</returns>
        /// <remarks>Only accessible to Admins and Librarians.</remarks>
        [HttpPost("ensure"), Authorize(Roles = UserRoles.Admin + "," + UserRoles.Librarian)]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Ensure a full location (v1)",
            Description = "Creates or retrieves a complete location (floor, aisle, shelf, and level).",
            Tags = ["Locations"]
        )]
        [ProducesResponseType(typeof(LocationReadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<LocationReadDto>> Ensure(
            [FromBody] LocationEnsureDto? dto,
            CancellationToken ct)
        {
            if (dto is null) return BadRequest();

            var result = await _svc.EnsureAsync(dto, ct);
            return Ok(result);
        }
    }
}

