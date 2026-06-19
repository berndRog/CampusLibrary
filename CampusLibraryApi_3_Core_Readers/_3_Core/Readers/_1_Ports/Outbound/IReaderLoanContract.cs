using CampusLibraryApi._2_BuildingBlocks;
using CampusLibraryApi._3_Core.Readers._2_Application.Dtos;
namespace CampusLibraryApi._3_Core.Readers._1_Ports.Outbound;

// Public module interface for loan-related reader checks.
// The Loans module may use this interface, but it does not access
// the Readers table or Reader aggregate directly.
public interface IReaderLoanContract {

   Task<Result<ReaderLoanInfoDto>> FindActiveReaderForLoanAsync(
      Guid readerId,
      CancellationToken ct
   );
}