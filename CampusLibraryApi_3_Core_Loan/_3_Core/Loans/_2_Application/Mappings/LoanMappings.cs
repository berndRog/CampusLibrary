using CampusLibraryApi._3_Core.Loans._2_Application.Dtos;
using CampusLibraryApi._3_Core.Loans._3_Domain.Entities;
namespace CampusLibraryApi._3_Core.Loans._2_Application.Mappings;

public static class LoanMappings {
   
   public static LoanDto ToLoanDto(this Loan loan) => new(
      Id: loan.Id,
      LoanDate: loan.LoanDate,
      DueDate: loan.DueDate,
      ReaderId: loan.ReaderId,
      BookItemId: loan.BookItemId,
      ReturnedAt: loan.ReturnedAt,
      Status: loan.Status,
      RenewalCount: loan.RenewalCount
   );
   
}