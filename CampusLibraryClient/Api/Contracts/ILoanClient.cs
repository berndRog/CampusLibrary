using CampusLibraryClient.Api.Dtos;
using CampusLibraryClient.Core;

namespace CampusLibraryClient.Api.Contracts;

public interface ILoanClient {

   Task<Result<IEnumerable<LoanListItemDto>>> GetBorrowedAsync(
      CancellationToken ct = default
   );

   Task<Result<LoanDetailDto>> GetByIdAsync(
      Guid id,
      CancellationToken ct = default
   );

   Task<Result<LoanDto>> BorrowAsync(
      LoanCreateDto dto,
      CancellationToken ct = default
   );

   Task<Result<LoanDto>> ReturnAtDeskAsync(
      Guid id,
      CancellationToken ct = default
   );

   Task<Result<LoanDto>> RenewAsync(
      Guid id,
      CancellationToken ct = default
   );
}
