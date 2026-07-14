using CampusLibraryApi._2_BuildingBlocks;
using CampusLibraryApi._2_BuildingBlocks._1_Ports;
using CampusLibraryApi._3_Core.Catalog._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Catalog._2_Application.Dtos;
using CampusLibraryApi._3_Core.Catalog._2_Application.Mappings;
using CampusLibraryApi._3_Core.Catalog._3_Domain.Errors;
using Microsoft.Extensions.Logging;

using CampusLibraryApi._2_BuildingBlocks._1_Ports.Contracts;
namespace CampusLibraryApi._3_Core.Catalog._2_Application.UseCases;

public sealed class BookUcDeactivate(
   IBookRepository bookRepository,
   ILoanCatalogContract loanCatalogContract,
   IUnitOfWork unitOfWork,
   IClock clock,
   ILogger<BookUcDeactivate> logger
) {

   public async Task<Result<BookDto>> ExecuteAsync(
      Guid bookId,
      CancellationToken ct = default
   ) {
      if(bookId == Guid.Empty)
         return Result<BookDto>.Failure(CatalogErrors.InvalidBookId);

      // Load the Book aggregate including its BookItems.
      var book = await bookRepository.FindByIdAsync(
         id: bookId,
         ct: ct
      );

      if(book is null)
         return Result<BookDto>.Failure(CatalogErrors.BookNotFound);

      Guid[] bookItemIds = book.BookItems
         .Select(bookItem => bookItem.Id)
         .ToArray();

      bool hasCurrentLoans = await loanCatalogContract.ExistsForBookItemsAsync(
         bookItemIds: bookItemIds,
         ct: ct
      );

      if(hasCurrentLoans)
         return Result<BookDto>.Failure(
            CatalogErrors.BookCannotBeDeactivatedWithLoans
         );

      // Deactivation removes all BookItems from the aggregate. EF Core deletes
      // the required child entities as orphans when SaveChanges is called.
      var resultDeactivated = book.Deactivate(
         updatedAt: clock.UtcNow
      );

      if(resultDeactivated.IsFailure)
         return Result<BookDto>.Failure(resultDeactivated.Error);

      var rows = await unitOfWork.SaveAllChangesAsync(
         "BookUcDeactivate",
         ct
      );

      logger.LogDebug(
         "BookUcDeactivate completed {BookId}, removed {BookItemCount} items, rows: {Rows}.",
         book.Id,
         bookItemIds.Length,
         rows
      );

      return Result<BookDto>.Success(book.ToBookDto());
   }
}
