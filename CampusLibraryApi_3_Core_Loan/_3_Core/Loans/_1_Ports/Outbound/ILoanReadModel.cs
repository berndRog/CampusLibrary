using CampusLibraryApi._2_BuildingBlocks;
using CampusLibraryApi._3_Core.Loans._2_Application.Dtos;
namespace CampusLibraryApi._3_Core.Loans._1_Ports.Outbound;

public interface ILoanReadModel {
   Task<Result<LoanDetailDto>> FindByIdAsync(
      Guid loanId,
      DateTime utcNow,
      CancellationToken ct = default
   );

   Task<Result<LoanDetailDto>> FindByIdForReaderAsync(
      Guid loanId,
      Guid readerId,
      DateTime utcNow,
      CancellationToken ct = default
   );

   Task<Result<IEnumerable<LoanListItemDto>>> SelectActiveByReaderIdAsync(
      Guid readerId,
      DateTime utcNow,
      CancellationToken ct = default
   );

   Task<Result<IEnumerable<LoanListItemDto>>> SelectOverdueByReaderIdAsync(
      Guid readerId,
      DateTime utcNow,
      CancellationToken ct = default
   );
}
