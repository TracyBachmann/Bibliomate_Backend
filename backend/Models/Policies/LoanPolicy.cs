namespace backend.Models.Policies
{
    public static class LoanPolicy
    {
        public const int MaxActiveLoansPerUser = 5;
        public const int DefaultLoanDurationDays = 14;
        public const decimal LateFeePerDay = 0.5m;    
    }
}