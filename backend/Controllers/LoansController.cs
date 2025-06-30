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

        /// <summary>
        /// Retrieves all loans in the system.
        /// </summary>
        /// <returns>
        /// 200 OK with a list of Loan objects,
        /// 400 BadRequest with { error } on failure.
        /// </returns>
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _loanService.GetAllAsync();
            if (result.IsError)
                return BadRequest(new { error = result.Error });

            return Ok(result.Value);
        }

        /// <summary>
        /// Retrieves a specific loan by its identifier.
        /// </summary>
        /// <param name="id">Identifier of the loan to retrieve.</param>
        /// <returns>
        /// 200 OK with the Loan object,
        /// 400 BadRequest with { error } if not found or on failure.
        /// </returns>
        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _loanService.GetByIdAsync(id);
            if (result.IsError)
                return BadRequest(new { error = result.Error });

            return Ok(result.Value);
        }

        /// <summary>
        /// Updates fields of an existing loan (e.g., due date). Only Librarians and Admins may call this.
        /// </summary>
        /// <param name="id">Identifier of the loan to update.</param>
        /// <param name="dto">Data for updating the loan.</param>
        /// <returns>
        /// 200 OK with the updated Loan object,
        /// 400 BadRequest with { error } on failure.
        /// </returns>
        [Authorize(Roles = $"{UserRoles.Librarian},{UserRoles.Admin}")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateLoan(int id, LoanUpdateDto dto)
        {
            var result = await _loanService.UpdateAsync(id, dto);
            if (result.IsError)
                return BadRequest(new { error = result.Error });

            return Ok(result.Value);
        }

        /// <summary>
        /// Deletes an existing loan. Only Librarians and Admins may call this.
        /// </summary>
        /// <param name="id">Identifier of the loan to delete.</param>
        /// <returns>
        /// 200 OK with { message } on success,
        /// 400 BadRequest with { error } on failure.
        /// </returns>
        [Authorize(Roles = $"{UserRoles.Librarian},{UserRoles.Admin}")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLoan(int id)
        {
            var result = await _loanService.DeleteAsync(id);
            if (result.IsError)
                return BadRequest(new { error = result.Error });

            return Ok(new { message = "Loan deleted successfully." });
        }
    }
}