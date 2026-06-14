using CampusLibraryApi._2_BuildingBlocks;
using CampusLibraryApi._2_BuildingBlocks._1_Ports;
using CampusLibraryApi._3_Core.Catalog._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Catalog._2_Application.Dtos;
using CampusLibraryApi._3_Core.Catalog._2_Application.Mappings;
using CampusLibraryApi._3_Core.Catalog._3_Domain.Errors;
using Microsoft.Extensions.Logging;

namespace CampusLibraryApi._3_Core.Catalog._2_Application.UseCases;

public sealed class BookUcDeactivate(
   IBookRepository bookRepository,
   IUnitOfWork unitOfWork,
   IClock clock,
   ILogger<BookUcDeactivate> logger
) {

   public async Task<Result<BookDto>> ExecuteAsync(
      Guid bookId,
      CancellationToken ct = default
   ) {
      if (bookId == Guid.Empty)
         return Result<BookDto>.Failure(CatalogErrors.InvalidBookId);

      // Load the Book aggregate.
      // With a global query filter, this returns active books only.
      var book = await bookRepository.FindByIdAsync(
         id: bookId,
         ct: ct
      );

      if (book is null)
         return Result<BookDto>.Failure(CatalogErrors.BookNotFound);

      // The aggregate controls its active state.
      var resultDeactivated = book.Deactivate(
         updatedAt: clock.UtcNow
      );

      if (resultDeactivated.IsFailure)
         return Result<BookDto>.Failure(resultDeactivated.Error);

      // No repository.Update(book) is needed.
      // The aggregate was loaded by EF Core and is already tracked.
      var rows = await unitOfWork.SaveAllChangesAsync(
         "BookUcDeactivate",
         ct
      );

      logger.LogDebug(
         "BookUcDeactivate completed {BookId}, rows: {Rows}.",
         book.Id, rows);

      return Result<BookDto>.Success(book.ToBookDto());
   }
}