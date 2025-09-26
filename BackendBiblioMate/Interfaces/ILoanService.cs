using BackendBiblioMate.DTOs;
using BackendBiblioMate.Models;

namespace BackendBiblioMate.Interfaces
{
    /// <summary>
    /// Defines business operations related to book loans,
    /// including creation, return handling, retrieval, update, and deletion.
    /// </summary>
    /// <remarks>
    /// This contract abstracts the loan domain logic from the persistence layer.
    /// It enforces validation rules (e.g., loan policies, stock availability) and
    /// ensures consistent domain behavior across implementations.
    /// </remarks>
    public interface ILoanService
    {
        /// <summary>
        /// Creates a new loan for a given book and user.
        /// </summary>
        /// <param name="dto">
        /// Data transfer object containing loan details (user identifier, book identifier, etc.).
        /// </param>
        /// <param name="cancellationToken">
        /// Token to observe for cancellation requests.
        /// </param>
        /// <returns>
        /// A <see cref="Result{TSuccess,TError}"/> where:
        /// <list type="bullet">
        ///   <item><description><c>Value</c>: <see cref="LoanCreatedResult"/> with the due date if successful.</description></item>
        ///   <item><description><c>Error</c>: a descriptive error message if creation fails (e.g., invalid user, no stock available).</description></item>
        /// </list>
        /// </returns>
        Task<Result<LoanCreatedResult, string>> CreateAsync(
            LoanCreateDto dto,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Marks an existing loan as returned.
        /// Updates stock availability and may trigger reservation notifications.
        /// </summary>
        /// <param name="loanId">The unique identifier of the loan to return.</param>
        /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
        /// <returns>
        /// A <see cref="Result{TSuccess,TError}"/> where:
        /// <list type="bullet">
        ///   <item><description><c>Value</c>: <see cref="LoanReturnedResult"/> with fine and notification status if successful.</description></item>
        ///   <item><description><c>Error</c>: an error message if the loan was not found or already returned.</description></item>
        /// </list>
        /// </returns>
        Task<Result<LoanReturnedResult, string>> ReturnAsync(
            int loanId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves all existing loans in the system.
        /// </summary>
        /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
        /// <returns>
        /// A <see cref="Result{TSuccess,TError}"/> where:
        /// <list type="bullet">
        ///   <item><description><c>Value</c>: an <see cref="IEnumerable{Loan}"/> containing all loans.</description></item>
        ///   <item><description><c>Error</c>: an error message if retrieval fails.</description></item>
        /// </list>
        /// </returns>
        Task<Result<IEnumerable<Loan>, string>> GetAllAsync(
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a loan by its unique identifier.
        /// </summary>
        /// <param name="loanId">The identifier of the loan to retrieve.</param>
        /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
        /// <returns>
        /// A <see cref="Result{TSuccess,TError}"/> where:
        /// <list type="bullet">
        ///   <item><description><c>Value</c>: the <see cref="Loan"/> if found.</description></item>
        ///   <item><description><c>Error</c>: an error message if the loan was not found.</description></item>
        /// </list>
        /// </returns>
        Task<Result<Loan, string>> GetByIdAsync(
            int loanId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing loan (e.g., changes to due date).
        /// </summary>
        /// <param name="loanId">The identifier of the loan to update.</param>
        /// <param name="dto">Data transfer object containing updated loan details.</param>
        /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
        /// <returns>
        /// A <see cref="Result{TSuccess,TError}"/> where:
        /// <list type="bullet">
        ///   <item><description><c>Value</c>: the updated <see cref="Loan"/> if successful.</description></item>
        ///   <item><description><c>Error</c>: an error message if the loan could not be updated.</description></item>
        /// </list>
        /// </returns>
        Task<Result<Loan, string>> UpdateAsync(
            int loanId,
            LoanUpdateDto dto,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Permanently deletes a loan by its identifier.
        /// </summary>
        /// <param name="loanId">The identifier of the loan to delete.</param>
        /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
        /// <returns>
        /// A <see cref="Result{TSuccess,TError}"/> where:
        /// <list type="bullet">
        ///   <item><description><c>Value</c>: <c>true</c> if the loan was successfully deleted.</description></item>
        ///   <item><description><c>Error</c>: an error message if the loan was not found or could not be deleted.</description></item>
        /// </list>
        /// </returns>
        Task<Result<bool, string>> DeleteAsync(
            int loanId,
            CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Represents the result of an operation that can either succeed with a value
    /// or fail with an error.
    /// </summary>
    /// <typeparam name="TSuccess">Type of the value returned on success.</typeparam>
    /// <typeparam name="TError">Type of the error returned on failure.</typeparam>
    public sealed class Result<TSuccess, TError>
    {
        /// <summary>
        /// Gets the value if the operation succeeded; otherwise <c>default</c>.
        /// </summary>
        public TSuccess? Value { get; }

        /// <summary>
        /// Gets the error if the operation failed; otherwise <c>default</c>.
        /// </summary>
        public TError? Error { get; }

        /// <summary>
        /// Gets a value indicating whether this result represents an error.
        /// </summary>
        public bool IsError => Error is not null;

        /// <summary>
        /// Initializes a new instance of the <see cref="Result{TSuccess,TError}"/> class.
        /// </summary>
        /// <param name="value">The success value (if any).</param>
        /// <param name="error">The error value (if any).</param>
        private Result(TSuccess? value, TError? error)
        {
            Value = value;
            Error = error;
        }

        /// <summary>
        /// Creates a successful result containing the specified value.
        /// </summary>
        /// <param name="value">The success value.</param>
        /// <returns>A new <see cref="Result{TSuccess,TError}"/> in success state.</returns>
        public static Result<TSuccess, TError> Ok(TSuccess value) => new(value, default);

        /// <summary>
        /// Creates a failed result containing the specified error.
        /// </summary>
        /// <param name="error">The error value.</param>
        /// <returns>A new <see cref="Result{TSuccess,TError}"/> in error state.</returns>
        public static Result<TSuccess, TError> Fail(TError error) => new(default, error);
    }
}
