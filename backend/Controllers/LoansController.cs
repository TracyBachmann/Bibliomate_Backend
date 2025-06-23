using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using backend.DTOs;
using backend.Services;
using backend.Models.Policies;
using Microsoft.AspNetCore.Authorization;
using backend.Helpers;
using backend.Models.Enums;

namespace backend.Controllers
{
    /// <summary>
    /// Controller for managing book loans.
    /// Provides endpoints for creating, updating, returning, and listing loans.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class LoansController : ControllerBase
    {
        private readonly BiblioMateDbContext _context;
        private readonly StockService _stockService;
        private readonly NotificationService _notificationService;

        public LoansController(BiblioMateDbContext context, StockService stockService, NotificationService notificationService)
        {
            _context = context;
            _stockService = stockService;
            _notificationService = notificationService;
        }

        /// <summary>
        /// GET: api/Loans  
        /// Retrieves all loans, including related book and user information.  
        /// Only accessible to Admins and Librarians.
        /// </summary>
        [Authorize(Roles = $"{UserRoles.Librarian},{UserRoles.Admin}")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LoanReadDto>>> GetLoans()
        {
            var loans = await _context.Loans
                .Include(l => l.User)
                .Include(l => l.Book)
                .ToListAsync();

            return Ok(loans.Select(ToLoanReadDto));
        }

        /// <summary>
        /// GET: api/Loans/{id}  
        /// Retrieves a specific loan by ID.  
        /// Only accessible to Admins and Librarians.
        /// </summary>
        /// <param name="id">The ID of the loan.</param>
        [Authorize(Roles = $"{UserRoles.Librarian},{UserRoles.Admin}")]
        [HttpGet("{id}")]
        public async Task<ActionResult<LoanReadDto>> GetLoan(int id)
        {
            var loan = await _context.Loans
                .Include(l => l.User)
                .Include(l => l.Book)
                .FirstOrDefaultAsync(l => l.LoanId == id);

            return loan == null ? NotFound() : Ok(ToLoanReadDto(loan));
        }

        /// <summary>
        /// GET: api/Loans/user/{userId}  
        /// Retrieves loans by user ID.  
        /// Regular users can only access their own loans.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        [Authorize]
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<LoanReadDto>>> GetLoansByUser(int userId)
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

            return Ok(loans.Select(ToLoanReadDto));
        }

        /// <summary>
        /// POST: api/Loans  
        /// Creates a new loan if user is eligible and stock is available.  
        /// Only accessible to Admins and Librarians.
        /// </summary>
        /// <param name="dto">Loan creation data.</param>
        [Authorize(Roles = $"{UserRoles.Librarian},{UserRoles.Admin}")]
        [HttpPost]
        public async Task<ActionResult> CreateLoan(LoanCreateDto dto)
        {
            var user = await _context.Users.FindAsync(dto.UserId);
            if (user == null)
                return BadRequest(new { error = "User not found." });

            int activeLoans = await _context.Loans.CountAsync(l =>
                l.UserId == dto.UserId && l.ReturnDate == null);

            if (activeLoans >= LoanPolicy.MaxActiveLoansPerUser)
                return BadRequest(new { error = $"The maximum number of active loans ({LoanPolicy.MaxActiveLoansPerUser}) is already reached." });

            var stock = await _context.Stocks.FirstOrDefaultAsync(s => s.BookId == dto.BookId);
            if (stock == null || stock.Quantity <= 0)
                return BadRequest(new { error = "The requested book is currently unavailable." });

            var loan = new Loan
            {
                UserId = dto.UserId,
                BookId = dto.BookId,
                LoanDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(LoanPolicy.DefaultLoanDurationDays)
            };

            _context.Loans.Add(loan);
            _stockService.Decrease(stock);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Loan created successfully.",
                dueDate = loan.DueDate
            });
        }

        /// <summary>
        /// PUT: api/Loans/{id}  
        /// Updates an existing loan.  
        /// Only accessible to Admins and Librarians.
        /// </summary>
        /// <param name="id">The ID of the loan to update.</param>
        /// <param name="dto">Updated loan data.</param>
        [Authorize(Roles = $"{UserRoles.Librarian},{UserRoles.Admin}")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateLoan(int id, LoanUpdateDto dto)
        {
            if (id != dto.LoanId)
                return BadRequest();

            var loan = await _context.Loans.FindAsync(id);
            if (loan == null)
                return NotFound();

            loan.BookId = dto.BookId;
            loan.UserId = dto.UserId;
            loan.LoanDate = dto.LoanDate;
            loan.DueDate = dto.DueDate;
            loan.ReturnDate = dto.ReturnDate;
            loan.Fine = dto.Fine;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// DELETE: api/Loans/{id}  
        /// Deletes a loan by ID.  
        /// Only accessible to Admins and Librarians.
        /// </summary>
        /// <param name="id">The loan ID to delete.</param>
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

        /// <summary>
        /// PUT: api/Loans/{id}/return  
        /// Marks a loan as returned, updates stock and calculates fine if late.  
        /// Only accessible to Admins and Librarians.
        /// </summary>
        /// <param name="id">The ID of the loan to return.</param>
        [Authorize(Roles = $"{UserRoles.Librarian},{UserRoles.Admin}")]
        [HttpPut("{id}/return")]
        public async Task<IActionResult> ReturnLoan(int id)
        {
            var loan = await _context.Loans
                .Include(l => l.Stock)
                .ThenInclude(s => s.Book)
                .FirstOrDefaultAsync(l => l.LoanId == id);

            if (loan == null)
                return NotFound("Emprunt non trouvé.");

            if (loan.ReturnDate != null)
                return BadRequest(new { error = "Livre déjà rendu." });

            loan.ReturnDate = DateTime.UtcNow;

            var stock = loan.Stock;
            stock.IsAvailable = true;

            var reservation = await _context.Reservations
                .Where(r => r.BookId == stock.BookId && r.Status == ReservationStatus.Pending)
                .OrderBy(r => r.CreatedAt)
                .FirstOrDefaultAsync();

            bool reservationNotified = false;

            if (reservation != null)
            {
                reservation.AssignedStockId = stock.StockId;
                reservation.Status = ReservationStatus.Available;
                stock.IsAvailable = false;

                var bookTitle = stock.Book?.Title ?? "Inconnu";

                await _notificationService.NotifyUser(
                    reservation.UserId,
                    $"📚 Le livre '{bookTitle}' est désormais disponible pour vous. Vous avez 48h pour venir le récupérer."
                );

                reservationNotified = true;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Livre rendu avec succès.",
                reservationNotified
            });
        }

        /// <summary>
        /// Maps a <see cref="Loan"/> entity to its corresponding <see cref="LoanReadDto"/>.
        /// </summary>
        private static LoanReadDto ToLoanReadDto(Loan loan) => new()
        {
            LoanId     = loan.LoanId,
            UserId     = loan.UserId,
            UserName   = loan.User?.Name ?? "Unknown",
            BookId     = loan.BookId,
            BookTitle  = loan.Book?.Title ?? "Unknown",
            LoanDate   = loan.LoanDate,
            DueDate    = loan.DueDate,
            ReturnDate = loan.ReturnDate,
            Fine       = loan.Fine
        };
    }
}
