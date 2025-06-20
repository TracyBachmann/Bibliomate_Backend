using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Authorization;

namespace backend.Controllers
{
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
        /// <summary>
        /// Retrieves all loans.
        /// Only Admins and Librarians.
        /// </summary>
        [Authorize(Roles = "Librarian,Admin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Loan>>> GetLoans()
        {
            return await _context.Loans
                .Include(l => l.User)
                .Include(l => l.Book)
                .ToListAsync();
        }

        // GET: api/Loans/5
        /// <summary>
        /// Retrieves a specific loan by ID.
        /// Only Admins and Librarians.
        /// </summary>
        [Authorize(Roles = "Librarian,Admin")]
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

        // GET: api/Loans/user/5
        /// <summary>
        /// Retrieves all loans for a specific user.
        /// Users can see their own loans. Admins/Librarians can see all.
        /// </summary>
        [Authorize]
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<Loan>>> GetLoansByUser(int userId)
        {
            var currentUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

            if (currentUserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Librarian"))
                return Forbid();

            var loans = await _context.Loans
                .Include(l => l.Book)
                .Where(l => l.UserId == userId)
                .ToListAsync();

            if (!loans.Any())
                return NotFound("Aucun emprunt n'a été trouvé pour cet utilisateur.");

            return Ok(loans);
        }

        // POST: api/Loans
        /// <summary>
        /// Creates a new loan.
        /// Only Admins and Librarians.
        /// </summary>
        [Authorize(Roles = "Librarian,Admin")]
        [HttpPost]
        public async Task<ActionResult> CreateLoan(Loan loan)
        {
            var user = await _context.Users.FindAsync(loan.UserId);
            if (user == null)
                return BadRequest(new { error = "Utilisateur non trouvé." });

            int activeLoans = await _context.Loans.CountAsync(l =>
                l.UserId == loan.UserId && l.ReturnDate == null);

            if (activeLoans >= 5)
                return BadRequest(new { error = "Le nombre maximum d'emprunts actifs (5) est déjà atteint." });

            var stock = await _context.Stocks.FirstOrDefaultAsync(s => s.BookId == loan.BookId);
            if (stock == null || stock.Quantity <= 0)
                return BadRequest(new { error = "Le livre n'est actuellement pas disponible." });

            loan.LoanDate = DateTime.UtcNow;
            loan.DueDate = DateTime.UtcNow.AddDays(14);

            _context.Loans.Add(loan);
            _stockService.Decrease(stock);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Emprunt créé avec succès.",
                dueDate = loan.DueDate
            });
        }

        // PUT: api/Loans/5
        /// <summary>
        /// Updates a loan.
        /// Only Admins and Librarians.
        /// </summary>
        [Authorize(Roles = "Librarian,Admin")]
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
                else
                    throw;
            }

            return NoContent();
        }

        // DELETE: api/Loans/5
        /// <summary>
        /// Deletes a loan.
        /// Only Admins and Librarians.
        /// </summary>
        [Authorize(Roles = "Librarian,Admin")]
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

        // PUT: api/Loans/5/return
        /// <summary>
        /// Marks a loan as returned, calculates any late fine, and updates stock.
        /// Only Admins and Librarians.
        /// </summary>
        [Authorize(Roles = "Librarian,Admin")]
        [HttpPut("{id}/return")]
        public async Task<IActionResult> ReturnBook(int id)
        {
            var loan = await _context.Loans
                .Include(l => l.Book)
                .FirstOrDefaultAsync(l => l.LoanId == id);

            if (loan == null)
                return NotFound("Emprunt non trouvé.");

            if (loan.ReturnDate != null)
                return BadRequest(new { error = "Livre déjà retourné." });

            loan.ReturnDate = DateTime.UtcNow;

            int daysLate = (loan.ReturnDate.Value - loan.DueDate).Days;
            loan.Fine = daysLate > 0 ? daysLate * 0.5f : 0;

            var stock = await _context.Stocks.FirstOrDefaultAsync(s => s.BookId == loan.Book.BookId);
            if (stock != null)
                _stockService.Increase(stock);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Livre retourné avec succès.",
                fine = loan.Fine,
                daysLate,
                returnDate = loan.ReturnDate
            });
        }
    }
}
