using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using backend.Models.Dtos;
using Microsoft.AspNetCore.Authorization;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StocksController : ControllerBase
    {
        private readonly BiblioMateDbContext _context;

        public StocksController(BiblioMateDbContext context)
        {
            _context = context;
        }

        // GET: api/Stocks
        /// <summary>
        /// Retrieves all stock entries, with optional pagination.
        /// </summary>
        /// <param name="page">Page number (default is 1).</param>
        /// <param name="pageSize">Items per page (default is 10).</param>
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
                .FirstOrDefaultAsync(s => s.Id == id);

            if (stock == null)
                return NotFound();

            return stock;
        }

        // POST: api/Stocks
        /// <summary>
        /// Creates a new stock entry. Only Admins and Librarians are allowed.
        /// Prevents duplicates for the same Book.
        /// </summary>
        [Authorize(Roles = "Admin,Librarian")]
        [HttpPost]
        public async Task<ActionResult<Stock>> CreateStock(Stock stock)
        {
            // Check if a stock already exists for this BookId
            var existingStock = await _context.Stocks.FirstOrDefaultAsync(s => s.BookId == stock.BookId);
            if (existingStock != null)
            {
                return Conflict(new { message = "Stock already exists for this book. You should update the quantity instead." });
            }

            _context.Stocks.Add(stock);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetStock), new { id = stock.Id }, stock);
        }

        // PUT: api/Stocks/5
        /// <summary>
        /// Updates an existing stock entry. Only Admins and Librarians are allowed.
        /// </summary>
        [Authorize(Roles = "Admin,Librarian")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStock(int id, Stock stock)
        {
            if (id != stock.Id)
                return BadRequest();

            _context.Entry(stock).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Stocks.Any(s => s.Id == id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }
        
        // PATCH /api/Stocks/{id}/adjust
        /// <summary>
        /// Adjusts the quantity of an existing stock entry (increment or decrement). Only Admins and Librarians are allowed.
        /// </summary>
        /// <param name="id">ID of the stock to adjust.</param>
        /// <param name="dto">DTO containing the adjustment value.</param>
        [Authorize(Roles = "Admin,Librarian")]
        [HttpPatch("{id}/adjust")]
        public async Task<IActionResult> AdjustStockQuantity(int id, [FromBody] StockAdjustmentDto dto)
        {
            if (dto.Adjustment == 0)
                return BadRequest(new { message = "Adjustment value cannot be zero." });

            var stock = await _context.Stocks.FindAsync(id);
            if (stock == null)
                return NotFound();

            stock.Quantity += dto.Adjustment;

            if (stock.Quantity < 0)
                return BadRequest(new { message = "Stock quantity cannot be negative." });

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Stock updated successfully.",
                newQuantity = stock.Quantity
            });
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