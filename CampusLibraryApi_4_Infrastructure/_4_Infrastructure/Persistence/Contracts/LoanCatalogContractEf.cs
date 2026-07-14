using System.Runtime.CompilerServices;
using CampusLibraryApi._2_BuildingBlocks;
using CampusLibraryApi._2_BuildingBlocks._1_Ports;
using CampusLibraryApi._2_BuildingBlocks._2_Application.Contracts;
using CampusLibraryApi._3_Core.Readers._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Loans._1_Ports.Outbound;
using Microsoft.EntityFrameworkCore;

using CampusLibraryApi._2_BuildingBlocks._1_Ports.Contracts;
[assembly: InternalsVisibleTo("CampusLibraryApiTest")]
namespace CampusLibraryApi._4_Infrastructure.Persistence.Contracts;

internal sealed class LoanCatalogContractEf(
   ILoanDbContext loanDbContext,
   IReaderReadModel readerReadModel
) : ILoanCatalogContract {

   public async Task<bool> ExistsForBookItemsAsync(
      IReadOnlyCollection<Guid> bookItemIds,
      CancellationToken ct
   ) {
      if(bookItemIds.Count == 0)
         return false;

      return await loanDbContext.Loans
         .AsNoTracking()
         .AnyAsync(
            loan => bookItemIds.Contains(loan.BookItemId),
            ct
         );
   }

   public async Task<Result<IReadOnlyList<CurrentBookItemLoanInfoDto>>> FindCurrentLoansForBookItemsAsync(
      IReadOnlyCollection<Guid> bookItemIds,
      CancellationToken ct
   ) {
      if(bookItemIds.Count == 0)
         return Result<IReadOnlyList<CurrentBookItemLoanInfoDto>>.Success([]);

      List<CurrentLoanProjection> loans = await loanDbContext.Loans
         .AsNoTracking()
         .Where(loan => bookItemIds.Contains(loan.BookItemId))
         .OrderBy(loan => loan.LoanPeriodVo.DueDate)
         .Select(loan => new CurrentLoanProjection(
            loan.BookItemId,
            loan.ReaderId,
            loan.LoanPeriodVo.DueDate
         ))
         .ToListAsync(ct);

      List<CurrentBookItemLoanInfoDto> dtos = [];

      foreach(CurrentLoanProjection loan in loans) {
         var readerResult = await readerReadModel.FindByIdAsync(
            id: loan.ReaderId,
            includeInactive: true,
            ct: ct
         );

         if(readerResult.IsFailure)
            return Result<IReadOnlyList<CurrentBookItemLoanInfoDto>>.Failure(
               readerResult.Error
            );

         dtos.Add(
            new CurrentBookItemLoanInfoDto(
               BookItemId: loan.BookItemId,
               ReaderEmail: readerResult.Value.Email,
               DueDate: loan.DueDate
            )
         );
      }

      return Result<IReadOnlyList<CurrentBookItemLoanInfoDto>>.Success(dtos);
   }

   private sealed record CurrentLoanProjection(
      Guid BookItemId,
      Guid ReaderId,
      DateTime DueDate
   );

}
