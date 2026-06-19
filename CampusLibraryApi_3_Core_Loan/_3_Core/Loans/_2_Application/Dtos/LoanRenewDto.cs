namespace CampusLibraryApi._3_Core.Loans._2_Application.Dtos;

public sealed record LoanRenewDto(
   Guid ReaderId,
   Guid LoanId
);
