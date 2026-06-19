using CampusLibraryApi._2_BuildingBlocks;
using CampusLibraryApi._3_Core.Readers._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Readers._2_Application.Dtos;
using CampusLibraryApi._3_Core.Readers._2_Application.Mappings;
using CampusLibraryApi._3_Core.Readers._3_Domain.Errors;
using Microsoft.EntityFrameworkCore;
namespace CampusLibraryApi._4_Infrastructure.Readers.Gateways;

internal sealed class ReaderLoanContractEf(
   IReaderDbContext dbContext
) : IReaderLoanContract {
   public async Task<Result<ReaderLoanInfoDto>> FindActiveReaderForLoanAsync(
      Guid readerId,
      CancellationToken ct
   ) {
      // Query Readers only inside the Readers access implementation.
      // Loans never sees the Reader table or Reader aggregate.
      var reader = await dbContext.Readers
         .SingleOrDefaultAsync(r => r.Id == readerId, ct);

      return reader is null
         ? Result<ReaderLoanInfoDto>.Failure(ReaderErrors.ReaderNotFound)
         : Result<ReaderLoanInfoDto>.Success(reader.ToReaderLoanInfoDto());
   }
}