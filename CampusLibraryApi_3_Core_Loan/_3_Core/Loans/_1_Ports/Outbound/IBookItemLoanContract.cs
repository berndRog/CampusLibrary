using CampusLibraryApi._2_BuildingBlocks;
namespace CampusLibraryApi._3_Core.Loans._1_Ports.Outbound;

public interface IBookItemLoanContract {
   Task<Result<bool>> ExistsAsync(
      Guid bookItemId,
      CancellationToken ct = default
   );

   Task<Result<bool>> IsAvailableAsync(
      Guid bookItemId,
      CancellationToken ct = default
   );

   Task<Result> MarkBorrowedAsync(
      Guid bookItemId,
      DateTime updatedAt,
      CancellationToken ct = default
   );

   Task<Result> MarkAvailableAsync(
      Guid bookItemId,
      DateTime updatedAt,
      CancellationToken ct = default
   );
}
