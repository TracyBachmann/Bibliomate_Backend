using BackendBiblioMate.Data;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Models;
using BackendBiblioMate.Models.Enums;
using BackendBiblioMate.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendBiblioMate.Controllers
{
    /// <summary>
    /// Controller for managing stock entries (inventory).
    /// Provides CRUD operations and quantity adjustments for <see cref="StockReadDto"/>.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
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
            _context = context;
            _stockService = stockService;
        }

        /// <summary>
        /// Retrieves all stock entries with optional pagination.
        /// </summary>
        /// <param name="page">Page index (1-based). Default is <c>1</c>.</param>
        /// <param name="pageSize">Items per page. Default is <c>10</c>.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>200 OK</c> with a paginated list of <see cref="StockReadDto"/>.
        /// </returns>
        [HttpGet, Authorize]
        [ProducesResponseType(typeof(IEnumerable<StockReadDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<StockReadDto>>> GetStocks(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var stocks = await _context.Stocks
                .Include(s => s.Book)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return Ok(stocks.Select(MapToDto));
        }

        /// <summary>
        /// Retrieves a specific stock entry by its identifier.
        /// </summary>
        /// <param name="id">The stock identifier.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>200 OK</c> with <see cref="StockReadDto"/>,  
        /// <c>404 NotFound</c> if not found.
        /// </returns>
        [HttpGet("{id}"), Authorize]
        [ProducesResponseType(typeof(StockReadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<StockReadDto>> GetStock(
            [FromRoute] int id,
            CancellationToken cancellationToken = default)
        {
            var stock = await _context.Stocks
                .Include(s => s.Book)
                .FirstOrDefaultAsync(s => s.StockId == id, cancellationToken);

            if (stock is null)
                return NotFound();

            return Ok(MapToDto(stock));
        }

        /// <summary>
        /// Creates a new stock entry.
        /// </summary>
        /// <param name="dto">The stock creation payload.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>201 Created</c> with the created <see cref="StockReadDto"/> and location header;  
        /// <c>409 Conflict</c> if a stock entry already exists for the given book;  
        /// <c>401 Unauthorized</c> or <c>403 Forbidden</c> if access denied.
        /// </returns>
        [HttpPost, Authorize(Roles = UserRoles.Admin + "," + UserRoles.Librarian)]
        [ProducesResponseType(typeof(StockReadDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<StockReadDto>> CreateStock(
            [FromBody] StockCreateDto dto,
            CancellationToken cancellationToken = default)
        {
            if (await _context.Stocks.AnyAsync(s => s.BookId == dto.BookId, cancellationToken))
            {
                return Conflict(new { message = "A stock entry already exists for that book." });
            }

            var entity = new Stock
            {
                BookId   = dto.BookId,
                Quantity = dto.Quantity
            };
            _stockService.UpdateAvailability(entity);

            _context.Stocks.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            // Reload Book navigation for title
            await _context.Entry(entity).Reference(e => e.Book).LoadAsync(cancellationToken);

            var result = MapToDto(entity);
            return CreatedAtAction(nameof(GetStock), new { id = result.StockId }, result);
        }

        /// <summary>
        /// Updates an existing stock entry.
        /// </summary>
        /// <param name="id">The stock entry ID to update.</param>
        /// <param name="dto">The updated stock data.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>204 NoContent</c> on success;  
        /// <c>400 BadRequest</c> if the ID does not match;  
        /// <c>404 NotFound</c> if not found;  
        /// <c>401 Unauthorized</c> or <c>403 Forbidden</c> if access denied.
        /// </returns>
        [HttpPut("{id}"), Authorize(Roles = UserRoles.Admin + "," + UserRoles.Librarian)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UpdateStock(
            [FromRoute] int id,
            [FromBody] StockUpdateDto dto,
            CancellationToken cancellationToken = default)
        {
            if (id != dto.StockId)
                return BadRequest("Route ID and payload StockId do not match.");

            // Exécution directe de l'UPDATE en base, sans passer par l'entité C# en mémoire
            var rows = await _context.Stocks
                .Where(s => s.StockId == id)
                .ExecuteUpdateAsync(updates => updates
                        .SetProperty(s => s.BookId,     dto.BookId)
                        .SetProperty(s => s.Quantity,   dto.Quantity)
                        .SetProperty(s => s.IsAvailable, dto.IsAvailable),
                    cancellationToken);

            if (rows == 0)
                return NotFound();

            return NoContent();
        }

        /// <summary>
        /// Adjusts the quantity and availability of a stock entry.
        /// </summary>
        /// <param name="id">The stock entry ID.</param>
        /// <param name="dto">The adjustment payload (positive or negative).</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>200 OK</c> with the new quantity;  
        /// <c>400 BadRequest</c> for invalid adjustments;  
        /// <c>404 NotFound</c> if not found;  
        /// <c>401 Unauthorized</c> or <c>403 Forbidden</c> if access denied.
        /// </returns>
        [HttpPatch("{id}/adjust"), Authorize(Roles = UserRoles.Admin + "," + UserRoles.Librarian)]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> AdjustStockQuantity(
            [FromRoute] int id,
            [FromBody] StockAdjustmentDto dto,
            CancellationToken cancellationToken = default)
        {
            if (dto.Adjustment == 0)
                return BadRequest(new { message = "Adjustment cannot be zero." });

            var entity = await _context.Stocks.FindAsync(new object[] { id }, cancellationToken);
            if (entity is null)
                return NotFound();

            if (entity.Quantity + dto.Adjustment < 0)
                return BadRequest(new { message = "Resulting quantity cannot be negative." });

            _stockService.AdjustQuantity(entity, dto.Adjustment);
            await _context.SaveChangesAsync(cancellationToken);

            return Ok(new { message = "Stock updated successfully.", newQuantity = entity.Quantity });
        }

        /// <summary>
        /// Deletes a stock entry.
        /// </summary>
        /// <param name="id">The stock entry ID to delete.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>204 NoContent</c> on success;  
        /// <c>404 NotFound</c> if not found;  
        /// <c>401 Unauthorized</c> or <c>403 Forbidden</c> if access denied.
        /// </returns>
        [HttpDelete("{id}"), Authorize(Roles = UserRoles.Admin + "," + UserRoles.Librarian)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeleteStock(
            [FromRoute] int id,
            CancellationToken cancellationToken = default)
        {
            var entity = await _context.Stocks.FindAsync(new object[] { id }, cancellationToken);
            if (entity is null)
                return NotFound();

            _context.Stocks.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
            return NoContent();
        }

        /// <summary>
        /// Maps a <see cref="Stock"/> entity to its DTO.
        /// </summary>
        private static StockReadDto MapToDto(Stock s) => new()
        {
            StockId     = s.StockId,
            BookId      = s.BookId,
            BookTitle   = s.Book.Title,
            Quantity    = s.Quantity,
            IsAvailable = s.IsAvailable
        };
    }
}