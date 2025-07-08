using BackendBiblioMate.DTOs;
using BackendBiblioMate.Models;

namespace BackendBiblioMate.Interfaces
{
    /// <summary>
    /// Defines business operations related to book loans.
    /// </summary>
    public interface ILoanService
    {
        /// <summary>
        /// Creates a new loan.
        /// </summary>
        /// <param name="dto">Data transfer object containing loan details.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="T:System.Threading.Tasks.Task{BackendBiblioMate.Interfaces.Result{LoanCreatedResult,string}}"/>
        /// that yields a <see cref="T:BackendBiblioMate.Interfaces.Result{LoanCreatedResult,string}"/>.
        /// On success, <c>Value</c> contains a <see cref="T:BackendBiblioMate.Interfaces.LoanCreatedResult"/>; on failure, <c>Error</c> contains an error message.
        /// </returns>
        Task<Result<LoanCreatedResult, string>> CreateAsync(
            LoanCreateDto dto,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Marks a loan as returned.
        /// </summary>
        /// <param name="loanId">Identifier of the loan to return.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="T:System.Threading.Tasks.Task{BackendBiblioMate.Interfaces.Result{LoanReturnedResult,string}}"/>
        /// that yields a <see cref="T:BackendBiblioMate.Interfaces.Result{LoanReturnedResult,string}"/>.
        /// On success, <c>Value</c> contains a <see cref="T:BackendBiblioMate.Interfaces.LoanReturnedResult"/>; on failure, <c>Error</c> contains an error message.
        /// </returns>
        Task<Result<DTOs.LoanReturnedResult, string>>   ReturnAsync(
            int loanId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves all loans.
        /// </summary>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="T:System.Threading.Tasks.Task{BackendBiblioMate.Interfaces.Result{System.Collections.Generic.IEnumerable{BackendBiblioMate.Models.Loan},string}}"/>
        /// that yields a <see cref="T:BackendBiblioMate.Interfaces.Result{System.Collections.Generic.IEnumerable{BackendBiblioMate.Models.Loan},string}"/>.
        /// On success, <c>Value</c> contains the collection of <see cref="T:BackendBiblioMate.Models.Loan"/>; on failure, <c>Error</c> contains an error message.
        /// </returns>
        Task<Result<IEnumerable<Loan>, string>> GetAllAsync(
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a loan by its identifier.
        /// </summary>
        /// <param name="loanId">Identifier of the loan to retrieve.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="T:System.Threading.Tasks.Task{BackendBiblioMate.Interfaces.Result{BackendBiblioMate.Models.Loan,string}}"/>
        /// that yields a <see cref="T:BackendBiblioMate.Interfaces.Result{BackendBiblioMate.Models.Loan,string}"/>.
        /// On success, <c>Value</c> contains the <see cref="T:BackendBiblioMate.Models.Loan"/>; on failure, <c>Error</c> contains an error message.
        /// </returns>
        Task<Result<Loan, string>> GetByIdAsync(
            int loanId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing loan.
        /// </summary>
        /// <param name="loanId">Identifier of the loan to update.</param>
        /// <param name="dto">Data transfer object with updated loan details.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="T:System.Threading.Tasks.Task{BackendBiblioMate.Interfaces.Result{BackendBiblioMate.Models.Loan,string}}"/>
        /// yielding a <see cref="T:BackendBiblioMate.Interfaces.Result{BackendBiblioMate.Models.Loan,string}"/>.
        /// On success, <c>Value</c> contains the updated <see cref="T:BackendBiblioMate.Models.Loan"/>; on failure, <c>Error</c> contains an error message.
        /// </returns>
        Task<Result<Loan, string>> UpdateAsync(
            int loanId,
            LoanUpdateDto dto,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a loan by its identifier.
        /// </summary>
        /// <param name="loanId">Identifier of the loan to delete.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="T:System.Threading.Tasks.Task{BackendBiblioMate.Interfaces.Result{bool,string}}"/>
        /// yielding a <see cref="T:BackendBiblioMate.Interfaces.Result{System.Boolean,string}"/>.
        /// On success, <c>Value</c> is <c>true</c>; on failure, <c>Error</c> contains an error message.
        /// </returns>
        Task<Result<bool, string>> DeleteAsync(
            int loanId,
            CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Holds the result data for a successfully created loan.
    /// </summary>
    public record LoanCreatedResult
    {
        /// <summary>
        /// Due date of the loan.
        /// </summary>
        public DateTime DueDate { get; init; }
    }

    /// <summary>
    /// Holds the result data for a successfully returned loan.
    /// </summary>
    public record LoanReturnedResult
    {
        /// <summary>
        /// Indicates whether reservation notification was sent.
        /// </summary>
        public bool ReservationNotified { get; init; }
    }

    /// <summary>
    /// Represents either a success (<typeparamref name="TSuccess"/>) or an error (<typeparamref name="TError"/>).
    /// </summary>
    /// <typeparam name="TSuccess">Type of the success value.</typeparam>
    /// <typeparam name="TError">Type of the error value.</typeparam>
    public sealed class Result<TSuccess, TError>
    {
        /// <summary>
        /// Gets the success value, or <c>default</c> if an error occurred.
        /// </summary>
        public TSuccess? Value { get; }

        /// <summary>
        /// Gets the error value, or <c>default</c> if the operation was successful.
        /// </summary>
        public TError? Error { get; }

        /// <summary>
        /// Gets a value indicating whether the result represents an error.
        /// </summary>
        public bool IsError => Error is not null;

        private Result(TSuccess? value, TError? error)
        {
            Value = value;
            Error = error;
        }

        /// <summary>
        /// Creates a successful result.
        /// </summary>
        /// <param name="value">The success value.</param>
        /// <returns>A <see cref="T:BackendBiblioMate.Interfaces.Result{TSuccess,TError}"/> representing success.</returns>
        public static Result<TSuccess, TError> Ok(TSuccess value)
            => new(value, default);

        /// <summary>
        /// Creates an error result.
        /// </summary>
        /// <param name="error">The error value.</param>
        /// <returns>A <see cref="T:BackendBiblioMate.Interfaces.Result{TSuccess,TError}"/> representing an error.</returns>
        public static Result<TSuccess, TError> Fail(TError error)
            => new(default, error);
    }
}