using BackendBiblioMate.Data;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Interfaces;
using BackendBiblioMate.Models;
using BackendBiblioMate.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace BackendBiblioMate.Controllers
{
    /// <summary>
    /// Controller for managing stock entries (inventory).
    /// Provides CRUD operations and quantity adjustments for <see cref="StockReadDto"/>.
    /// </summary>
    [ApiController]
    [Route("api/[controller]"), Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    public class StocksController : ControllerBase
    {
        private readonly BiblioMateDbContext _context;
        private readonly IStockService _stockService;

        /// <summary>
        /// Initializes a new instance of <see cref="StocksController"/>.
        /// </summary>
        /// <param name="context">EF Core database context.</param>
        /// <param name="stockService">Service encapsulating stock domain logic.</param>
        public StocksController(
            BiblioMateDbContext context,
            IStockService stockService)
        {
            _context      = context ?? throw new ArgumentNullException(nameof(context));
            _stockService = stockService ?? throw new ArgumentNullException(nameof(stockService));
        }

        /// <summary>
        /// Retrieves all stock entries with optional pagination.
        /// </summary>
        [HttpGet, Authorize]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Retrieves all stock entries (v1)",
            Description = "Supports optional pagination.",
            Tags = ["Stocks"]
        )]
        [ProducesResponseType(typeof(IEnumerable<StockReadDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<StockReadDto>>> GetStocks(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var stocks = await _context.Stocks
                .Include(s => s.Book)
                .OrderBy(s => s.StockId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var dtos = stocks.Select(s => new StockReadDto
            {
                StockId     = s.StockId,
                BookId      = s.BookId,
                BookTitle   = s.Book.Title,
                Quantity    = s.Quantity,
                IsAvailable = s.IsAvailable
            });

            return Ok(dtos);
        }

        /// <summary>
        /// Retrieves a specific stock entry by its identifier.
        /// </summary>
        [HttpGet("{id}"), Authorize]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Retrieves a stock entry by ID (v1)",
            Description = "Returns a single stock entry.",
            Tags = ["Stocks"]
        )]
        [ProducesResponseType(typeof(StockReadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<StockReadDto>> GetStock(
            [FromRoute] int id,
            CancellationToken cancellationToken = default)
        {
            var s = await _context.Stocks
                .Include(x => x.Book)
                .FirstOrDefaultAsync(x => x.StockId == id, cancellationToken);

            if (s == null)
                return NotFound();

            var dto = new StockReadDto
            {
                StockId     = s.StockId,
                BookId      = s.BookId,
                BookTitle   = s.Book.Title,
                Quantity    = s.Quantity,
                IsAvailable = s.IsAvailable
            };

            return Ok(dto);
        }

        /// <summary>
        /// Creates a new stock entry.
        /// </summary>
        [HttpPost, Authorize(Roles = UserRoles.Admin + "," + UserRoles.Librarian)]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Creates a new stock entry (v1)",
            Description = "Accessible to Librarians and Admins only.",
            Tags = ["Stocks"]
        )]
        [ProducesResponseType(typeof(StockReadDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<StockReadDto>> CreateStock(
            [FromBody] StockCreateDto dto,
            CancellationToken cancellationToken = default)
        {
            if (await _context.Stocks.AnyAsync(s => s.BookId == dto.BookId, cancellationToken))
                return Conflict(new { message = "A stock entry already exists for that book." });

            var entity = new Stock
            {
                BookId   = dto.BookId,
                Quantity = dto.Quantity
            };
            _stockService.UpdateAvailability(entity);

            _context.Stocks.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);
            await _context.Entry(entity).Reference(e => e.Book).LoadAsync(cancellationToken);

            var result = new StockReadDto
            {
                StockId     = entity.StockId,
                BookId      = entity.BookId,
                BookTitle   = entity.Book.Title,
                Quantity    = entity.Quantity,
                IsAvailable = entity.IsAvailable
            };

            return CreatedAtAction(
                nameof(GetStock),
                new { id = result.StockId },
                result);
        }

        /// <summary>
        /// Updates an existing stock entry.
        /// </summary>
        [HttpPut("{id}"), Authorize(Roles = UserRoles.Admin + "," + UserRoles.Librarian)]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Updates a stock entry (v1)",
            Description = "Accessible to Librarians and Admins only.",
            Tags = ["Stocks"]
        )]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateStock(
            [FromRoute] int id,
            [FromBody] StockUpdateDto dto,
            CancellationToken cancellationToken = default)
        {
            if (id != dto.StockId)
                return BadRequest(new { error = "Route ID and payload StockId do not match." });

            var e = await _context.Stocks.FindAsync(new object[] { id }, cancellationToken);
            if (e == null)
                return NotFound();

            e.BookId   = dto.BookId;
            e.Quantity = dto.Quantity;
            await _context.SaveChangesAsync(cancellationToken);

            return NoContent();
        }

        /// <summary>
        /// Adjusts the quantity of a stock entry.
        /// </summary>
        [HttpPatch("{id}/adjustQuantity"), Authorize(Roles = UserRoles.Admin + "," + UserRoles.Librarian)]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Adjusts stock quantity (v1)",
            Description = "Allows incrementing or decrementing stock quantity.",
            Tags = ["Stocks"]
        )]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<object>> AdjustStockQuantity(
            [FromRoute] int id,
            [FromBody] StockAdjustmentDto dto,
            CancellationToken cancellationToken = default)
        {
            if (dto.Adjustment == 0)
                return BadRequest(new { message = "Adjustment cannot be zero." });

            var e = await _context.Stocks.FindAsync(new object[] { id }, cancellationToken);
            if (e == null)
                return NotFound();

            if (e.Quantity + dto.Adjustment < 0)
                return BadRequest(new { message = "Resulting quantity cannot be negative." });

            _stockService.AdjustQuantity(e, dto.Adjustment);
            await _context.SaveChangesAsync(cancellationToken);

            return Ok(new
            {
                message     = "Stock updated successfully.",
                newQuantity = e.Quantity
            });
        }

        /// <summary>
        /// Deletes a stock entry.
        /// </summary>
        [HttpDelete("{id}"), Authorize(Roles = UserRoles.Admin + "," + UserRoles.Librarian)]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Deletes a stock entry (v1)",
            Description = "Accessible to Librarians and Admins only.",
            Tags = ["Stocks"]
        )]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteStock(
            [FromRoute] int id,
            CancellationToken cancellationToken = default)
        {
            var e = await _context.Stocks.FindAsync(new object[] { id }, cancellationToken);
            if (e == null)
                return NotFound();

            _context.Stocks.Remove(e);
            await _context.SaveChangesAsync(cancellationToken);
            return NoContent();
        }
    }
}