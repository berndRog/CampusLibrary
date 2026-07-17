using System.Runtime.CompilerServices;
using CampusLibraryApi._2_BuildingBlocks;
using CampusLibraryApi._3_Core.Loans._1_Ports.Inbound;
using CampusLibraryApi._3_Core.Loans._2_Application.Dtos;

[assembly: InternalsVisibleTo("CampusLibraryApiTest")]
namespace CampusLibraryApi._3_Core.Loans._2_Application.UseCases;

internal sealed class LoanUseCases(
   LoanUcBorrow loanUcBorrow,
   LoanUcRenew loanUcRenew,
   LoanUcReturnAtDesk loanUcReturnAtDesk
) : ILoanUseCases {

   public Task<Result<Guid>> BorrowAsync(
      LoanCreateDto dto,
      CancellationToken ct = default
   ) => loanUcBorrow.ExecuteAsync(dto, ct);

   public Task<Result<Guid>> RenewAsync(
      Guid loanId,
      CancellationToken ct = default
   ) => loanUcRenew.ExecuteAsync(loanId, ct);

   public Task<Result> ReturnAtDeskAsync(
      Guid loanId,
      CancellationToken ct = default
   ) => loanUcReturnAtDesk.ExecuteAsync(loanId, ct);
}