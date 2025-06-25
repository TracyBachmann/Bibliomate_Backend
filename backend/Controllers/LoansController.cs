using backend.DTOs;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.Models.Enums;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoansController : ControllerBase
    {
        private readonly ILoanService _loanService;

        /// <summary>
        /// Initializes a new instance of <see cref="LoansController"/>.
        /// </summary>
        /// <param name="loanService">Injected business service for loans.</param>
        public LoansController(ILoanService loanService)
        {
            _loanService = loanService;
        }

        /// <summary>
        /// Creates a new loan. Only Librarians and Admins may call this.
        /// </summary>
        /// <param name="dto">Data required to create the loan.</param>
        /// <returns>
        /// 200 OK with { message, dueDate } on success,  
        /// 400 BadRequest with { error } on failure.
        /// </returns>
        [Authorize(Roles = $"{UserRoles.Librarian},{UserRoles.Admin}")]
        [HttpPost]
        public async Task<IActionResult> CreateLoan(LoanCreateDto dto)
        {
            var result = await _loanService.CreateAsync(dto);
            if (result.IsError)
                return BadRequest(new { error = result.Error });

            return Ok(new
            {
                message = "Loan created successfully.",
                dueDate = result.Value!.DueDate
            });
        }

        /// <summary>
        /// Returns a book for an existing loan and notifies the next reservation if any.
        /// </summary>
        /// <param name="id">Identifier of the loan to return.</param>
        /// <returns>
        /// 200 OK with { message, reservationNotified } on success,  
        /// 400 BadRequest with { error } on failure.
        /// </returns>
        [Authorize(Roles = $"{UserRoles.Librarian},{UserRoles.Admin}")]
        [HttpPut("{id}/return")]
        public async Task<IActionResult> ReturnLoan(int id)
        {
            var result = await _loanService.ReturnAsync(id);
            if (result.IsError)
                return BadRequest(new { error = result.Error });

            return Ok(new
            {
                message             = "Book returned successfully.",
                reservationNotified = result.Value!.ReservationNotified
            });
        }

        // TODO: Add endpoints for GET, PUT (update), DELETE, each delegating to ILoanService
    }
}
