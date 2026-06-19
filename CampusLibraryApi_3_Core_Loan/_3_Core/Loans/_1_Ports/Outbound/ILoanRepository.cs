using CampusLibraryApi._2_BuildingBlocks;
using CampusLibraryApi._3_Core.Loans._3_Domain.Entities;
namespace CampusLibraryApi._3_Core.Loans._1_Ports.Outbound;

public interface ILoanRepository {
   Task<Result<Loan>> FindByIdAsync(
      Guid id,
      CancellationToken ct = default
   );

   Task<bool> ExistsActiveByBookItemIdAsync(
      Guid bookItemId,
      CancellationToken ct = default
   );

   Task<int> CountActiveByReaderIdAsync(
      Guid readerId,
      CancellationToken ct = default
   );

   Task InsertAsync(
      Loan loan,
      CancellationToken ct = default
   );

   Task UpdateAsync(
      Loan loan,
      CancellationToken ct = default
   );
}
