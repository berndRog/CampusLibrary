using CampusLibraryApi._2_BuildingBlocks;
using CampusLibraryApi._2_BuildingBlocks._1_Ports;
using CampusLibraryApi._2_BuildingBlocks._2_Application.Utils;
using CampusLibraryApi._3_Core.Catalog._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Catalog._2_Application.Dtos;
using CampusLibraryApi._3_Core.Catalog._2_Application.Mappings;
using CampusLibraryApi._3_Core.Catalog._3_Domain.Entities;
using CampusLibraryApi._3_Core.Catalog._3_Domain.Errors;

namespace CampusLibraryApi._3_Core.Catalog._2_Application.UseCases;

public sealed class BookUcCreate(
   IBookRepository bookRepository,
   IUnitOfWork unitOfWork,
   IClock clock
) {

   public async Task<Result<BookDto>> ExecuteAsync(
      BookCreateDto dto,
      CancellationToken ct = default
   ) {
      if (dto is null)
         return Result<BookDto>.Failure(CatalogErrors.BookCreateDtoRequired);

      // Resolve the optional external id into a domain id.
      var idResult = EntityId.Resolve(dto.Id, CatalogErrors.InvalidBookId);
      if (idResult.IsFailure)
         return Result<BookDto>.Failure(idResult.Error);

      // Create the aggregate first, so ISBN validation and title trimming are applied.
      var bookResult = Book.Create(
         id: idResult.Value,
         title: dto.Title ?? string.Empty,
         subtitle: dto.Subtitle,
         isbn: dto.Isbn ?? string.Empty,
         createdAt: clock.UtcNow
      );

      if (bookResult.IsFailure)
         return Result<BookDto>.Failure(bookResult.Error);

      var book = bookResult.Value;

      // ISBN uniqueness requires persistence knowledge and belongs to the use case.
      var exists = await bookRepository.ExistsByIsbnAsync(
         book.IsbnVo.Value,
         ct
      );

      if (exists)
         return Result<BookDto>.Failure(CatalogErrors.BookAlreadyExists);

      bookRepository.Add(book);
      
      var rows = await unitOfWork.SaveAllChangesAsync("BookUcCreate",ct);

      return Result<BookDto>.Success(book.ToBookDto());
   }
}