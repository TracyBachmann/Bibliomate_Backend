using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using backend.Services;
using backend.Models.Policies;
using Microsoft.AspNetCore.Authorization;
using backend.Helpers;
using backend.Models.Enums;

namespace backend.Controllers
{
    /// <summary>
    /// Controller for managing book loans.
    /// Provides endpoints for creating, updating, returning and listing loans.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class LoansController : ControllerBase
    {
        private readonly BiblioMateDbContext _context;
        private readonly StockService _stockService;

        public LoansController(BiblioMateDbContext context, StockService stockService)
        {
            _context = context;
            _stockService = stockService;
        }

        // GET: api/Loans
        [Authorize(Roles = $"{UserRoles.Librarian},{UserRoles.Admin}")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Loan>>> GetLoans()
        {
            return await _context.Loans
                .Include(l => l.User)
                .Include(l => l.Book)
                .ToListAsync();
        }

        // GET: api/Loans/{id}
        [Authorize(Roles = $"{UserRoles.Librarian},{UserRoles.Admin}")]
        [HttpGet("{id}")]
        public async Task<ActionResult<Loan>> GetLoan(int id)
        {
            var loan = await _context.Loans
                .Include(l => l.User)
                .Include(l => l.Book)
                .FirstOrDefaultAsync(l => l.LoanId == id);

            if (loan == null)
                return NotFound();

            return loan;
        }

        // GET: api/Loans/user/{userId}
        [Authorize]
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<Loan>>> GetLoansByUser(int userId)
        {
            var currentUserId = TokenHelper.GetUserId(User);

            if (currentUserId != userId && !User.IsInRole(UserRoles.Admin) && !User.IsInRole(UserRoles.Librarian))
                return Forbid();

            var loans = await _context.Loans
                .Include(l => l.Book)
                .Where(l => l.UserId == userId)
                .ToListAsync();

            if (!loans.Any())
                return NotFound("No loans were found for this user.");

            return Ok(loans);
        }

        // POST: api/Loans
        [Authorize(Roles = $"{UserRoles.Librarian},{UserRoles.Admin}")]
        [HttpPost]
        public async Task<ActionResult> CreateLoan(Loan loan)
        {
            var user = await _context.Users.FindAsync(loan.UserId);
            if (user == null)
                return BadRequest(new { error = "User not found." });

            int activeLoans = await _context.Loans.CountAsync(l =>
                l.UserId == loan.UserId && l.ReturnDate == null);

            if (activeLoans >= LoanPolicy.MaxActiveLoansPerUser)
                return BadRequest(new { error = $"The maximum number of active loans ({LoanPolicy.MaxActiveLoansPerUser}) is already reached." });

            var stock = await _context.Stocks.FirstOrDefaultAsync(s => s.BookId == loan.BookId);
            if (stock == null || stock.Quantity <= 0)
                return BadRequest(new { error = "The requested book is currently unavailable." });

            loan.LoanDate = DateTime.UtcNow;
            loan.DueDate = DateTime.UtcNow.AddDays(LoanPolicy.DefaultLoanDurationDays);

            _context.Loans.Add(loan);
            _stockService.Decrease(stock);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Loan created successfully.",
                dueDate = loan.DueDate
            });
        }

        // PUT: api/Loans/{id}
        [Authorize(Roles = $"{UserRoles.Librarian},{UserRoles.Admin}")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateLoan(int id, Loan loan)
        {
            if (id != loan.LoanId)
                return BadRequest();

            _context.Entry(loan).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Loans.Any(l => l.LoanId == id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        // DELETE: api/Loans/{id}
        [Authorize(Roles = $"{UserRoles.Librarian},{UserRoles.Admin}")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLoan(int id)
        {
            var loan = await _context.Loans.FindAsync(id);
            if (loan == null)
                return NotFound();

            _context.Loans.Remove(loan);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PUT: api/Loans/{id}/return
        [Authorize(Roles = $"{UserRoles.Librarian},{UserRoles.Admin}")]
        [HttpPut("{id}/return")]
        public async Task<IActionResult> ReturnBook(int id)
        {
            var loan = await _context.Loans
                .Include(l => l.Book)
                .FirstOrDefaultAsync(l => l.LoanId == id);

            if (loan == null)
                return NotFound("Loan not found.");

            if (loan.ReturnDate != null)
                return BadRequest(new { error = "Book already returned." });

            loan.ReturnDate = DateTime.UtcNow;

            int daysLate = (loan.ReturnDate.Value - loan.DueDate).Days;
            loan.Fine = daysLate > 0 ? daysLate * LoanPolicy.LateFeePerDay : 0;

            var stock = await _context.Stocks.FirstOrDefaultAsync(s => s.BookId == loan.Book.BookId);
            if (stock != null)
                _stockService.Increase(stock);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Book returned successfully.",
                fine = loan.Fine,
                daysLate,
                returnDate = loan.ReturnDate
            });
        }
    }
}
