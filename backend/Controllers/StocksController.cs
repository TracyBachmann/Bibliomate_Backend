using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using backend.Models.Dtos;
using backend.Services;
using Microsoft.AspNetCore.Authorization;

namespace backend.Controllers
{
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
        /// Retrieves all stock entries, with optional pagination.
        /// </summary>
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Stock>>> GetStocks(int page = 1, int pageSize = 10)
        {
            var stocks = await _context.Stocks
                .Include(s => s.Book)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(stocks);
        }

        // GET: api/Stocks/5
        /// <summary>
        /// Retrieves a specific stock entry by its ID.
        /// </summary>
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
        /// Creates a new stock entry. Only Admins and Librarians are allowed.
        /// </summary>
        [Authorize(Roles = "Admin,Librarian")]
        [HttpPost]
        public async Task<ActionResult<Stock>> CreateStock(Stock stock)
        {
            var existingStock = await _context.Stocks.FirstOrDefaultAsync(s => s.BookId == stock.BookId);
            if (existingStock != null)
            {
                return Conflict(new { message = "Une entrée de stock existe déjà pour ce livre. Merci de mettre à jour la quantité à la place." });
            }

            _stockService.UpdateAvailability(stock);
            _context.Stocks.Add(stock);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetStock), new { id = stock.StockId }, stock);
        }

        // PUT: api/Stocks/5
        /// <summary>
        /// Updates an existing stock entry. Only Admins and Librarians are allowed.
        /// </summary>
        [Authorize(Roles = "Admin,Librarian")]
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
                else
                    throw;
            }

            return NoContent();
        }

        // PATCH: api/Stocks/{id}/adjust
        /// <summary>
        /// Adjusts stock quantity and availability. Only Admins and Librarians are allowed.
        /// </summary>
        [Authorize(Roles = "Admin,Librarian")]
        [HttpPatch("{id}/adjust")]
        public async Task<IActionResult> AdjustStockQuantity(int id, [FromBody] StockAdjustmentDto dto)
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

            return Ok(new { message = "Stock mis à jour avec succès.", newQuantity = stock.Quantity });
        }

        // DELETE: api/Stocks/5
        /// <summary>
        /// Deletes a stock entry by ID. Only Admins and Librarians are allowed.
        /// </summary>
        [Authorize(Roles = "Admin,Librarian")]
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
