using System.Security.Claims;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Interfaces;
using BackendBiblioMate.Models.Enums;
using BackendBiblioMate.Models.Policies;
using BackendBiblioMate.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace BackendBiblioMate.Controllers
{
    /// <summary>
    /// Represents a simplified row of an active loan returned by <c>GET /api/v1/loans/active/me</c>.
    /// </summary>
    public sealed class LoanActiveRowDto
    {
        public int LoanId { get; set; }
        public int BookId { get; set; }
        public string BookTitle { get; set; } = default!;
        public string? CoverUrl { get; set; }
        public string? Description { get; set; }
        public DateTime LoanDate { get; set; }
        public DateTime? DueDate { get; set; }
    }

    /// <summary>
    /// API controller for managing book loans.
    /// Provides endpoints for creating, retrieving, updating, returning, extending, and deleting loans.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    public class LoansController : ControllerBase
    {
        private readonly ILoanService _loanService;
        private readonly ILogger<LoansController> _logger;
        private readonly IWebHostEnvironment _env;
        private readonly BiblioMateDbContext _db;

        public LoansController(
            ILoanService loanService,
            ILogger<LoansController> logger,
            IWebHostEnvironment env,
            BiblioMateDbContext db)
        {
            _loanService = loanService ?? throw new ArgumentNullException(nameof(loanService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _env = env ?? throw new ArgumentNullException(nameof(env));
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        // ---------- Helpers ----------

        private static int? TryGetUserIdFromClaims(ClaimsPrincipal user)
        {
            var claim = user.FindFirst(ClaimTypes.NameIdentifier)
                        ?? user.FindFirst("nameid")
                        ?? user.FindFirst("sub")
                        ?? user.FindFirst("uid");
            return int.TryParse(claim?.Value, out var id) ? id : null;
        }

        private async Task<LoanReadDto> ToDtoAsync(Models.Loan l, CancellationToken ct)
        {
            // Resolve User name
            string userName = l.User != null
                ? $"{l.User.FirstName} {l.User.LastName}".Trim()
                : (await _db.Users.AsNoTracking()
                    .Where(x => x.UserId == l.UserId)
                    .Select(x => new { x.FirstName, x.LastName })
                    .FirstOrDefaultAsync(ct)) is { } u
                    ? $"{u.FirstName} {u.LastName}".Trim()
                    : string.Empty;

            // Resolve Book title
            string bookTitle = l.Book?.Title
                ?? (await _db.Books.AsNoTracking()
                    .Where(x => x.BookId == l.BookId)
                    .Select(x => x.Title)
                    .FirstOrDefaultAsync(ct))
                ?? string.Empty;

            return new LoanReadDto
            {
                LoanId = l.LoanId,
                UserId = l.UserId,
                UserName = userName,
                BookId = l.BookId,
                BookTitle = bookTitle,
                LoanDate = l.LoanDate,
                DueDate = l.DueDate,
                ReturnDate = l.ReturnDate,
                Fine = l.Fine
            };
        }

        // ---------- Endpoints ----------

        [HttpPost]
        [Authorize]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(Summary = "Creates a new loan (v1)", Tags = ["Loans"])]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateLoan([FromBody] LoanCreateDto dto, CancellationToken ct = default)
        {
            if (dto.BookId <= 0)
                return BadRequest(new { error = "Invalid payload." });

            try
            {
                var isStaff = User.IsInRole(UserRoles.Librarian) || User.IsInRole(UserRoles.Admin);
                int? userIdFromToken = TryGetUserIdFromClaims(User);
                if (!isStaff && userIdFromToken is null)
                    return Unauthorized(new { error = "Authenticated user id not found in token." });

                var effectiveUserId = isStaff && dto.UserId > 0
                    ? dto.UserId
                    : userIdFromToken!.Value;

                var result = await _loanService.CreateAsync(
                    new LoanCreateDto { UserId = effectiveUserId, BookId = dto.BookId }, ct);

                if (result.IsError)
                    return BadRequest(new { error = result.Error });

                return Ok(new { message = "Loan created successfully.", dueDate = result.Value!.DueDate });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateLoan failed. Payload={@dto}", dto);
                return StatusCode(500, new
                {
                    error = "InternalError",
                    message = "An unexpected error occurred. Please try again later.",
                    details = _env.IsDevelopment() ? ex.ToString() : null
                });
            }
        }

        [HttpGet("debug/{bookId:int}")]
        [Authorize]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(Summary = "Debug stock & active loans (v1)", Tags = ["Loans"])]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DebugBook(int bookId, CancellationToken ct = default)
        {
            try
            {
                var uid = TryGetUserIdFromClaims(User);
                var stock = await _db.Stocks.AsNoTracking().FirstOrDefaultAsync(s => s.BookId == bookId, ct);
                var activeOnBook = await _db.Loans.AsNoTracking()
                    .CountAsync(l => l.BookId == bookId && l.ReturnDate == null, ct);
                var activeForUser = uid.HasValue
                    ? await _db.Loans.AsNoTracking().CountAsync(l => l.UserId == uid && l.ReturnDate == null, ct)
                    : 0;

                return Ok(new
                {
                    bookId,
                    stockQty = stock?.Quantity ?? 0,
                    activeLoansOnBook = activeOnBook,
                    activeLoansForCurrentUser = activeForUser,
                    policyMaxPerUser = LoanPolicy.MaxActiveLoansPerUser
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Loans debug failed for bookId={BookId}", bookId);
                return StatusCode(500, new
                {
                    error = "InternalError",
                    message = "Debug failed.",
                    details = _env.IsDevelopment() ? ex.ToString() : null
                });
            }
        }

        [HttpPut("{id}/return")]
        [Authorize(Roles = UserRoles.Librarian + "," + UserRoles.Admin)]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(Summary = "Marks a loan as returned (v1)", Tags = ["Loans"])]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ReturnLoan([FromRoute] int id, CancellationToken cancellationToken = default)
        {
            if (id <= 0) return BadRequest(new { error = "Invalid loan ID." });

            var result = await _loanService.ReturnAsync(id, cancellationToken);
            if (result.IsError) return BadRequest(new { error = result.Error });

            return Ok(new
            {
                message = "Book returned successfully.",
                reservationNotified = result.Value!.ReservationNotified,
                fine = result.Value.Fine
            });
        }

        [HttpGet]
        [Authorize]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(Summary = "Retrieves all loans (v1)", Tags = ["Loans"])]
        [ProducesResponseType(typeof(IEnumerable<LoanReadDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(CancellationToken ct = default)
        {
            var result = await _loanService.GetAllAsync(ct);
            if (result.IsError) return BadRequest(new { error = result.Error });

            var dtos = new List<LoanReadDto>();
            foreach (var l in result.Value!)
                dtos.Add(await ToDtoAsync(l, ct));

            return Ok(dtos);
        }

        [HttpGet("{id}")]
        [Authorize]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(Summary = "Retrieves a loan by ID (v1)", Tags = ["Loans"])]
        [ProducesResponseType(typeof(LoanReadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken ct = default)
        {
            if (id <= 0) return BadRequest(new { error = "Invalid loan ID." });

            var result = await _loanService.GetByIdAsync(id, ct);
            if (result.IsError) return BadRequest(new { error = result.Error });

            var dto = await ToDtoAsync(result.Value!, ct);
            return Ok(dto);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = UserRoles.Librarian + "," + UserRoles.Admin)]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(Summary = "Updates an existing loan (v1)", Tags = ["Loans"])]
        [ProducesResponseType(typeof(LoanReadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateLoan([FromRoute] int id, [FromBody] LoanUpdateDto dto, CancellationToken ct = default)
        {
            if (id <= 0) return BadRequest(new { error = "Invalid loan ID." });
            if (!ModelState.IsValid) return BadRequest(new { error = "Invalid payload." });

            var result = await _loanService.UpdateAsync(id, dto, ct);
            if (result.IsError) return BadRequest(new { error = result.Error });

            var updatedDto = await ToDtoAsync(result.Value!, ct);
            return Ok(updatedDto);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = UserRoles.Librarian + "," + UserRoles.Admin)]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(Summary = "Deletes a loan (v1)", Tags = ["Loans"])]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteLoan([FromRoute] int id, CancellationToken ct = default)
        {
            if (id <= 0) return BadRequest(new { error = "Invalid loan ID." });

            var result = await _loanService.DeleteAsync(id, ct);
            if (result.IsError) return BadRequest(new { error = result.Error });

            return Ok(new { message = "Loan deleted successfully." });
        }

        [HttpGet("active/me/{bookId:int}")]
        [Authorize]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(Summary = "Checks if current user has an active loan for the book (v1)", Tags = ["Loans"])]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> HasActiveForMe([FromRoute] int bookId, CancellationToken ct = default)
        {
            var uid = TryGetUserIdFromClaims(User);
            if (uid is null) return Unauthorized(new { error = "Authenticated user id not found in token." });

            var loan = await _db.Loans
                .AsNoTracking()
                .Where(l => l.UserId == uid.Value && l.BookId == bookId && l.ReturnDate == null)
                .OrderByDescending(l => l.LoanDate)
                .FirstOrDefaultAsync(ct);

            if (loan is null)
                return Ok(new { hasActive = false });

            return Ok(new { hasActive = true, dueDate = loan.DueDate });
        }

        [HttpGet("active/me")]
        [Authorize]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(Summary = "Lists current user's active loans (v1)", Tags = ["Loans"])]
        [ProducesResponseType(typeof(IEnumerable<LoanActiveRowDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMyActive(CancellationToken ct = default)
        {
            var uid = TryGetUserIdFromClaims(User);
            if (uid is null) return Unauthorized(new { error = "Authenticated user id not found in token." });

            var rows = await _db.Loans
                .AsNoTracking()
                .Where(l => l.UserId == uid.Value && l.ReturnDate == null)
                .OrderByDescending(l => l.LoanDate)
                .Select(l => new LoanActiveRowDto
                {
                    LoanId = l.LoanId,
                    BookId = l.BookId,
                    BookTitle = _db.Books.Where(b => b.BookId == l.BookId).Select(b => b.Title).FirstOrDefault()!,
                    CoverUrl = _db.Books.Where(b => b.BookId == l.BookId).Select(b => b.CoverUrl).FirstOrDefault(),
                    Description = _db.Books.Where(b => b.BookId == l.BookId).Select(b => b.Description).FirstOrDefault(),
                    LoanDate = l.LoanDate,
                    DueDate = l.DueDate
                })
                .ToListAsync(ct);

            return Ok(rows);
        }

        [HttpPost("{id:int}/extend")]
        [Authorize]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(Summary = "Extends an active loan (v1)", Tags = ["Loans"])]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ExtendLoan([FromRoute] int id, CancellationToken ct = default)
        {
            if (id <= 0) return BadRequest(new { error = "Invalid loan ID." });

            var loan = await _db.Loans.FirstOrDefaultAsync(l => l.LoanId == id, ct);
            if (loan is null) return NotFound(new { error = "Loan not found." });
            if (loan.ReturnDate is not null) return BadRequest(new { error = "Loan already returned." });

            var me = TryGetUserIdFromClaims(User);
            var isStaff = User.IsInRole(UserRoles.Admin) || User.IsInRole(UserRoles.Librarian);
            if (!isStaff && (!me.HasValue || me.Value != loan.UserId))
                return Forbid();

            if (loan.ExtensionsCount >= LoanPolicy.MaxExtensionsPerLoan)
                return BadRequest(new { error = "Maximum number of extensions reached." });

            loan.DueDate = loan.DueDate.AddDays(LoanPolicy.DefaultLoanDurationDays);
            loan.ExtensionsCount++;

            await _db.SaveChangesAsync(ct);

            return Ok(new { dueDate = loan.DueDate, extensions = loan.ExtensionsCount });
        }
    }
}

