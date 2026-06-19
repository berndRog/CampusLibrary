using CampusLibraryApi._2_BuildingBlocks;
using CampusLibraryApi._3_Core.Catalog._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Catalog._2_Application.Dtos;

namespace CampusLibraryApi._3_Core.Catalog._2_Application.UseCases;

public sealed class BookUseCases(
   BookUcCreate bookUcCreate,
   BookUcAddBookItem bookUcAddBookItem,
   BookUcDeactivate bookUcDeactivate
) : IBookUseCases {

   public async Task<Result<BookDto>> CreateAsync(
      BookCreateDto? dto,
      CancellationToken ct = default
   ) => await bookUcCreate.ExecuteAsync(
         bookCreateDto: dto,
         ct: ct
      );

   public async Task<Result<BookItemDto>> AddBookItemAsync(
      Guid bookId,
      BookItemAddDto? dto,
      CancellationToken ct = default
   ) => await bookUcAddBookItem.ExecuteAsync(
         bookId: bookId,
         bookItemAddDto: dto,
         ct: ct
      );
   
   public async Task<Result<BookDto>> DeactivateAsync(
      Guid bookId,
      CancellationToken ct = default
   ) => await bookUcDeactivate.ExecuteAsync(
         bookId: bookId,
         ct: ct
      );
}