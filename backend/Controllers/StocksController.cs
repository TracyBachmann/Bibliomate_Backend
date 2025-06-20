using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using backend.Models.Dtos;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using backend.Models.Enums;

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
        private readonly StockService _stockService;

        public StocksController(BiblioMateDbContext context, StockService stockService)
        {
            _context = context;
            _stockService = stockService;
        }

        // GET: api/Stocks
        /// <summary>
        /// Retrieves all stock entries with optional pagination.
        /// </summary>
        /// <param name="page">Page index (1-based). Default is <c>1</c>.</param>
        /// <param name="pageSize">Items per page. Default is <c>10</c>.</param>
        /// <returns>A paginated collection of <see cref="Stock"/>.</returns>
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Stock>>> GetStocks(
            int page = 1,
            int pageSize = 10)
        {
            var stocks = await _context.Stocks
                .Include(s => s.Book)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(stocks);
        }

        // GET: api/Stocks/{id}
        /// <summary>
        /// Retrieves a specific stock entry by its identifier.
        /// </summary>
        /// <param name="id">The stock identifier.</param>
        /// <returns>
        /// The requested stock entry if found; otherwise <c>404 NotFound</c>.
        /// </returns>
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<Stock>> GetStock(int id)
        {
            var stock = await _context.Stocks
                .Include(s => s.Book)
                .FirstOrDefaultAsync(s => s.StockId == id);

            if (stock == null)
                return NotFound();

            return stock;
        }

        // POST: api/Stocks
        /// <summary>
        /// Creates a new stock entry.
        /// Accessible to Librarians and Admins only.
        /// </summary>
        /// <param name="stock">The stock entity to create.</param>
        /// <returns>
        /// <c>201 Created</c> with the created stock and its URI;  
        /// <c>409 Conflict</c> if a stock entry already exists for the book.
        /// </returns>
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Librarian}")]
        [HttpPost]
        public async Task<ActionResult<Stock>> CreateStock(Stock stock)
        {
            var existingStock = await _context.Stocks
                .FirstOrDefaultAsync(s => s.BookId == stock.BookId);

            if (existingStock != null)
                return Conflict(new
                {
                    message =
                        "Une entrée de stock existe déjà pour ce livre. Merci de mettre à jour la quantité à la place."
                });

            _stockService.UpdateAvailability(stock);
            _context.Stocks.Add(stock);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetStock),
                new { id = stock.StockId }, stock);
        }

        // PUT: api/Stocks/{id}
        /// <summary>
        /// Updates an existing stock entry.
        /// Accessible to Librarians and Admins only.
        /// </summary>
        /// <param name="id">The identifier of the stock entry to update.</param>
        /// <param name="stock">The modified stock entity.</param>
        /// <returns>
        /// <c>204 NoContent</c> on success;  
        /// <c>400 BadRequest</c> if the IDs do not match;  
        /// <c>404 NotFound</c> if the stock entry does not exist.
        /// </returns>
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Librarian}")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStock(int id, Stock stock)
        {
            if (id != stock.StockId)
                return BadRequest();

            _stockService.UpdateAvailability(stock);
            _context.Entry(stock).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Stocks.Any(s => s.StockId == id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        // PATCH: api/Stocks/{id}/adjust
        /// <summary>
        /// Adjusts the quantity and availability of a stock entry.
        /// Accessible to Librarians and Admins only.
        /// </summary>
        /// <param name="id">The identifier of the stock entry.</param>
        /// <param name="dto">The adjustment payload (positive or negative integer).</param>
        /// <returns>
        /// <c>200 OK</c> with the new quantity;  
        /// <c>400 BadRequest</c> for invalid data;  
        /// <c>404 NotFound</c> if the stock entry does not exist.
        /// </returns>
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Librarian}")]
        [HttpPatch("{id}/adjust")]
        public async Task<IActionResult> AdjustStockQuantity(
            int id,
            [FromBody] StockAdjustmentDto dto)
        {
            if (dto.Adjustment == 0)
                return BadRequest(new { message = "La valeur ajustée ne peut pas être 0." });

            var stock = await _context.Stocks.FindAsync(id);
            if (stock == null)
                return NotFound();

            if (stock.Quantity + dto.Adjustment < 0)
                return BadRequest(new { message = "Le stock ne peut pas être négatif." });

            _stockService.AdjustQuantity(stock, dto.Adjustment);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message     = "Stock mis à jour avec succès.",
                newQuantity = stock.Quantity
            });
        }

        // DELETE: api/Stocks/{id}
        /// <summary>
        /// Deletes a stock entry.
        /// Accessible to Librarians and Admins only.
        /// </summary>
        /// <param name="id">The identifier of the stock entry to delete.</param>
        /// <returns>
        /// <c>204 NoContent</c> when deletion succeeds;  
        /// <c>404 NotFound</c> if the stock entry is not found.
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
