namespace BackendBiblioMate.Models.Policies
{
    /// <summary>
    /// Defines the loan policy constraints for the library system.
    /// Contains constants for maximum concurrent loans, default loan duration, and late fee calculation.
    /// </summary>
    public static class LoanPolicy
    {
        /// <summary>
        /// Gets the maximum number of active loans a user can have simultaneously.
        /// </summary>
        public const int MaxActiveLoansPerUser = 5;

        /// <summary>
        /// Gets the default number of days for a standard loan period.
        /// </summary>
        public const int DefaultLoanDurationDays = 14;

        /// <summary>
        /// Gets the fee amount charged per day for each overdue loan.
        /// </summary>
        public const decimal LateFeePerDay = 0.5m;
    }
}