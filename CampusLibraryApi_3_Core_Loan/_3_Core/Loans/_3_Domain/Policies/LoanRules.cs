namespace CampusLibraryApi._3_Core.Loans._3_Domain.Policies;

public static class LoanRules {
   public const int StandardLoanDays = 28;
   public const int StandardRenewalDays = 14;
   public const int MaxRenewals = 3;
}