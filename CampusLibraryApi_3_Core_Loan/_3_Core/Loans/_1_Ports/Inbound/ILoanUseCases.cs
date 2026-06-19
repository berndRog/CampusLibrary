using CampusLibraryApi._2_BuildingBlocks;
using CampusLibraryApi._3_Core.Loans._2_Application.Dtos;
namespace CampusLibraryApi._3_Core.Loans._1_Ports.Inbound;

public interface ILoanUseCases {
   Task<Result<LoanDetailDto>> BorrowAsync(
      LoanCreateDto dto,
      CancellationToken ct = default
   );

   Task<Result<LoanDetailDto>> ReturnAtDeskAsync(
      LoanReturnDto dto,
      CancellationToken ct = default
   );

   Task<Result<LoanDetailDto>> RenewAsync(
      LoanRenewDto dto,
      CancellationToken ct = default
   );
}
