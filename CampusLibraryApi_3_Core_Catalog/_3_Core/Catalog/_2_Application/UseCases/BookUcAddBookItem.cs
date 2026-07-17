using CampusLibraryApi._2_BuildingBlocks;
using CampusLibraryApi._2_BuildingBlocks._1_Ports;
using CampusLibraryApi._2_BuildingBlocks._2_Application.Utils;
using CampusLibraryApi._3_Core.Catalog._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Catalog._2_Application.Dtos;
using CampusLibraryApi._3_Core.Catalog._2_Application.Mappings;
using CampusLibraryApi._3_Core.Catalog._3_Domain.Errors;
using Microsoft.Extensions.Logging;

namespace CampusLibraryApi._3_Core.Catalog._2_Application.UseCases;

public sealed class BookUcAddBookItem(
   IBookRepository bookRepository,
   IUnitOfWork unitOfWork,
   IClock clock,
   ILogger<BookUcAddBookItem> logger
) {

   public async Task<Result<BookItemDto>> ExecuteAsync(
      Guid bookId,
      BookItemAddDto? bookItemAddDto,
      CancellationToken ct = default
   ) {
      if (bookId == Guid.Empty)
         return Result<BookItemDto>.Failure(CatalogErrors.InvalidBookId);

      if (bookItemAddDto is null)
         return Result<BookItemDto>.Failure(CatalogErrors.BookItemAddDtoRequired);
      var dto = bookItemAddDto!;

      // Load the Book aggregate including its existing BookItems.
      var book = await bookRepository.FindByIdAsync(bookId, ct);
      if (book is null)
         return Result<BookItemDto>.Failure(CatalogErrors.BookNotFound);

      // Resolve or generate the BookItem id.
      var resultId = EntityId.Resolve(dto.Id, CatalogErrors.InvalidBookItemId);
      if (resultId.IsFailure)
         return Result<BookItemDto>.Failure(resultId.Error);

      // The Book aggregate controls the BookItem creation.
      var resultBookItem = book.AddBookItem(
         bookItemId: resultId.Value,
         updatedAt: clock.UtcNow
      );

      if (resultBookItem.IsFailure)
         return Result<BookItemDto>.Failure(resultBookItem.Error);

      var bookItem = resultBookItem.Value;

      // Save all changes to the database:
      // added BookItem + updated Book.UpdatedAt.
      var rows = await unitOfWork.SaveAllChangesAsync(
         "BookUcAddBookItem", ct);

      logger.LogDebug(
         "BookUcAddBookItem completed for book item {BookItemId}. Saved rows: {Rows}.",
         bookItem.Id, rows);

      return Result<BookItemDto>.Success(bookItem.ToBookItemDto());
   }
}