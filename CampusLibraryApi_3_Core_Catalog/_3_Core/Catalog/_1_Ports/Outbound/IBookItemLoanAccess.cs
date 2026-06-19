using CampusLibraryApi._2_BuildingBlocks;
using CampusLibraryApi._3_Core.Catalog._2_Application.Dtos;
namespace CampusLibraryApi._3_Core.Catalog._1_Ports.Inbound;

// Public module interface for loan-related book item operations.
// The Loans module may use this interface, but it does not access
// Catalog tables or Catalog aggregates directly.
public interface IBookItemLoanAccess {

   Task<Result<BookItemLoanInfoDto>> FindAvailableBookItemForLoanAsync(
      Guid bookItemId,
      CancellationToken ct
   );

   Task<Result> MarkAsBorrowedAsync(
      Guid bookItemId,
      DateTime updatedAt,
      CancellationToken ct
   );

   Task<Result> MarkAsAvailableAsync(
      Guid bookItemId,
      DateTime updatedAt,
      CancellationToken ct
   );
}