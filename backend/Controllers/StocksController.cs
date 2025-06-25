using backend.Data;
using backend.DTOs;
using backend.Models;
using backend.Models.Enums;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers
{
    /// <summary>
    /// Controller for managing stock entries (inventory).
    /// Provides CRUD operations and quantity adjustments.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class StocksController : ControllerBase
    {
        private readonly BiblioMateDbContext _context;
        private readonly IStockService       _stockService;

        public StocksController(
            BiblioMateDbContext context,
            IStockService stockService)
        {
            _context      = context;
            _stockService = stockService;
        }

        /// <summary>
        /// Retrieves all stock entries with optional pagination.
        /// </summary>
        /// <param name="page">Page index (1-based). Default is <c>1</c>.</param>
        /// <param name="pageSize">Items per page. Default is <c>10</c>.</param>
        /// <returns>A paginated collection of <see cref="StockReadDto"/>.</returns>
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<StockReadDto>>> GetStocks(
            int page = 1,
            int pageSize = 10)
        {
            var stocks = await _context.Stocks
                .Include(s => s.Book)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

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
        /// <param name="id">The stock identifier.</param>
        /// <returns>
        /// The requested stock entry if found; otherwise <c>404 NotFound</c>.
        /// </returns>
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<StockReadDto>> GetStock(int id)
        {
            var s = await _context.Stocks
                .Include(s => s.Book)
                .FirstOrDefaultAsync(s => s.StockId == id);

            if (s == null) 
                return NotFound();

            return Ok(new StockReadDto
            {
                StockId     = s.StockId,
                BookId      = s.BookId,
                BookTitle   = s.Book.Title,
                Quantity    = s.Quantity,
                IsAvailable = s.IsAvailable
            });
        }

        /// <summary>
        /// Creates a new stock entry.
        /// Only accessible to Librarians and Admins.
        /// </summary>
        /// <param name="dto">The stock creation payload.</param>
        /// <returns>
        /// <c>201 Created</c> with the created stock;  
        /// <c>409 Conflict</c> if a stock entry already exists for the given book.
        /// </returns>
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Librarian}")]
        [HttpPost]
        public async Task<ActionResult<StockReadDto>> CreateStock(StockCreateDto dto)
        {
            if (await _context.Stocks.AnyAsync(s => s.BookId == dto.BookId))
            {
                return Conflict(new
                {
                    message = "A stock entry already exists for that book."
                });
            }

            var stock = new Stock
            {
                BookId   = dto.BookId,
                Quantity = dto.Quantity
            };
            _stockService.UpdateAvailability(stock);

            _context.Stocks.Add(stock);
            await _context.SaveChangesAsync();

            var book   = await _context.Books.FindAsync(stock.BookId);
            var result = new StockReadDto
            {
                StockId     = stock.StockId,
                BookId      = stock.BookId,
                BookTitle   = book?.Title ?? "Unknown",
                Quantity    = stock.Quantity,
                IsAvailable = stock.IsAvailable
            };

            return CreatedAtAction(
                nameof(GetStock),
                new { id = stock.StockId },
                result);
        }

        /// <summary>
        /// Updates an existing stock entry.
        /// Only accessible to Librarians and Admins.
        /// </summary>
        /// <param name="id">The stock entry ID to update.</param>
        /// <param name="dto">The updated stock data.</param>
        /// <returns>
        /// <c>204 NoContent</c> on success;  
        /// <c>400 BadRequest</c> if the ID does not match;  
        /// <c>404 NotFound</c> if the stock entry does not exist.
        /// </returns>
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Librarian}")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStock(int id, StockUpdateDto dto)
        {
            if (id != dto.StockId) 
                return BadRequest();

            var stock = await _context.Stocks.FindAsync(id);
            if (stock == null) 
                return NotFound();

            stock.BookId      = dto.BookId;
            stock.Quantity    = dto.Quantity;
            stock.IsAvailable = dto.IsAvailable;
            _stockService.UpdateAvailability(stock);

            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Adjusts the quantity and availability of a stock entry.
        /// Only accessible to Librarians and Admins.
        /// </summary>
        /// <param name="id">The stock entry ID.</param>
        /// <param name="dto">The adjustment payload (positive or negative).</param>
        /// <returns>
        /// <c>200 OK</c> with the new quantity;  
        /// <c>400 BadRequest</c> for invalid adjustments;  
        /// <c>404 NotFound</c> if the stock entry does not exist.
        /// </returns>
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Librarian}")]
        [HttpPatch("{id}/adjust")]
        public async Task<IActionResult> AdjustStockQuantity(
            int id,
            [FromBody] StockAdjustmentDto dto)
        {
            if (dto.Adjustment == 0)
                return BadRequest(new { message = "Adjustment cannot be zero." });

            var stock = await _context.Stocks.FindAsync(id);
            if (stock == null) 
                return NotFound();

            if (stock.Quantity + dto.Adjustment < 0)
                return BadRequest(new { message = "Resulting quantity cannot be negative." });

            _stockService.AdjustQuantity(stock, dto.Adjustment);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message     = "Stock updated successfully.",
                newQuantity = stock.Quantity
            });
        }

        /// <summary>
        /// Deletes a stock entry.
        /// Only accessible to Librarians and Admins.
        /// </summary>
        /// <param name="id">The stock entry ID to delete.</param>
        /// <returns>
        /// <c>204 NoContent</c> on success;  
        /// <c>404 NotFound</c> if not found.
        /// </returns>
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Librarian}")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStock(int id)
        {
            var stock = await _context.Stocks.FindAsync(id);
            if (stock == null) 
                return NotFound();

            _context.Stocks.Remove(stock);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
