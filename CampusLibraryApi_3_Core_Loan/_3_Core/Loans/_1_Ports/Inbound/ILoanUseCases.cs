using CampusLibraryApi._2_BuildingBlocks;
using CampusLibraryApi._3_Core.Loans._2_Application.Dtos;

namespace CampusLibraryApi._3_Core.Loans._1_Ports.Inbound;

// Inbound port for command use cases of the Loans module.
// Commands return only the changed aggregate id or success without exposing
// a second command-specific Loan DTO. HTTP responses are loaded through the
// read model afterwards.
public interface ILoanUseCases {
   Task<Result<Guid>> BorrowAsync(
      LoanCreateDto dto,
      CancellationToken ct
   );

   Task<Result<Guid>> RenewAsync(
      Guid loanId,
      CancellationToken ct
   );

   Task<Result> ReturnAtDeskAsync(
      Guid loanId,
      CancellationToken ct
   );
}
