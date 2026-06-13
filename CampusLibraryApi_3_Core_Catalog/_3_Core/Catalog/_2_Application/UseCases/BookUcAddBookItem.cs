using CampusLibraryApi._2_BuildingBlocks;
using CampusLibraryApi._2_BuildingBlocks._1_Ports;
using CampusLibraryApi._2_BuildingBlocks._2_Application.Utils;
using CampusLibraryApi._3_Core.Catalog._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Catalog._2_Application.Dtos;
using CampusLibraryApi._3_Core.Catalog._2_Application.Mappings;
using CampusLibraryApi._3_Core.Catalog._3_Domain.Errors;

namespace CampusLibraryApi._3_Core.Catalog._2_Application.UseCases;

public sealed class BookUcAddBookItem(
   IBookRepository bookRepository,
   IUnitOfWork unitOfWork,
   IClock clock
) {

   public async Task<Result<BookItemDto>> ExecuteAsync(
      Guid bookId,
      BookItemAddDto dto,
      CancellationToken ct = default
   ) {
      if(bookId == Guid.Empty)
         return Result<BookItemDto>.Failure(CatalogErrors.InvalidBookId);
      if (dto is null)
         return Result<BookItemDto>.Failure(CatalogErrors.BookItemAddDtoRequired);

      // load book by id
      var book = await bookRepository.FindByIdAsync(bookId, ct);
      if (book is null)
         return Result<BookItemDto>.Failure(CatalogErrors.BookNotFound);

      // Resolve or generate the book item id.
      var resultId = EntityId.Resolve(dto.Id, CatalogErrors.InvalidBookItemId);
      if (resultId.IsFailure)
         return Result<BookItemDto>.Failure(resultId.Error);
      var id = resultId.Value;
      
      // The Book aggregate controls the BookItem creation.
      var resultBookItem = book.AddBookItem(
         bookItemId: id,
         inventoryNumber: dto.InventoryNumber,
         updatedAt: clock.UtcNow
      );
      if (resultBookItem.IsFailure)
         return Result<BookItemDto>.Failure(resultBookItem.Error);
      var bookItem = resultBookItem.Value;

      var rows = await unitOfWork.SaveAllChangesAsync("BookUcAddBookItem",ct);

      return Result<BookItemDto>.Success(bookItem.ToBookItemDto());
   }
}