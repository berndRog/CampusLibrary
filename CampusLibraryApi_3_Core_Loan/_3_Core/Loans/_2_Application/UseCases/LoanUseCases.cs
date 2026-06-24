using System.Runtime.CompilerServices;
using CampusLibraryApi._2_BuildingBlocks;
using CampusLibraryApi._3_Core.Loans._1_Ports.Inbound;
using CampusLibraryApi._3_Core.Loans._2_Application.Dtos;
using CampusLibraryApi._3_Core.Loans._2_Application.UseCases;

[assembly: InternalsVisibleTo("CampusLibraryApiTest")]
namespace CampusLibraryApi._3_Core.Catalog._2_Application.UseCases;

internal sealed class LoanUseCases(
   LoanUcBorrow loanUcBorrow,
   LoanUcRenew loanUcRenew,
   LoanUcReturnAtDesk loanUcReturnAtDesk
) : ILoanUseCases {

   public async Task<Result<LoanDto>> BorrowAsync(
      LoanCreateDto? dto,
      CancellationToken ct = default
   ) => await loanUcBorrow.ExecuteAsync(
         loanCreateDto: dto,
         ct: ct
      );
   
   public async Task<Result<LoanDto>> RenewAsync(
      Guid loanId,
      CancellationToken ct = default
   ) => await loanUcRenew.ExecuteAsync(
         loanId: loanId,
         ct: ct
      );
   
   public async Task<Result<LoanDto>> ReturnAtDeskAsync(
      Guid loanId,
      CancellationToken ct = default
   ) => await loanUcReturnAtDesk.ExecuteAsync(
         loanId: loanId,
         ct: ct
      );
}