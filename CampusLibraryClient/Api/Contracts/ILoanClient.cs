using CampusLibraryClient.Api.Dtos;
using CampusLibraryClient.Core;

namespace CampusLibraryClient.Api.Contracts;

public interface ILoanClient {

   // Administrative list of all current loans.
   Task<Result<IEnumerable<LoanListItemDto>>> GetBorrowedAsync(
      CancellationToken ct = default
   );

   // Reader self-service list. The API derives the Reader from the token subject.
   Task<Result<IEnumerable<LoanListItemDto>>> GetMyBorrowedAsync(
      CancellationToken ct = default
   );

   // Administrative loan details.
   Task<Result<LoanDetailDto>> GetByIdAsync(
      Guid id,
      CancellationToken ct = default
   );

   // Reader self-service loan details.
   Task<Result<LoanDetailDto>> GetMyByIdAsync(
      Guid id,
      CancellationToken ct = default
   );

   Task<Result<LoanDto>> BorrowAsync(
      LoanCreateDto dto,
      CancellationToken ct = default
   );

   // Reader self-service borrow. The API derives the ReaderId from the token.
   Task<Result<LoanDto>> BorrowMyAsync(
      LoanBorrowMeDto dto,
      CancellationToken ct = default
   );

   Task<Result<LoanDto>> ReturnAtDeskAsync(
      Guid id,
      CancellationToken ct = default
   );

   // Administrative renewal endpoint.
   Task<Result<LoanDto>> RenewAsync(
      Guid id,
      CancellationToken ct = default
   );

   // Reader self-service renewal endpoint.
   Task<Result<LoanDto>> RenewMyAsync(
      Guid id,
      CancellationToken ct = default
   );
}
