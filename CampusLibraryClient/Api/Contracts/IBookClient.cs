using CampusLibraryClient.Api.Dtos;
using CampusLibraryClient.Core;

namespace CampusLibraryClient.Api.Contracts;

public interface IBookClient {

   Task<Result<IEnumerable<BookListItemDto>>> GetAllAsync(
      bool includeInactive = false,
      CancellationToken ct = default
   );

   Task<Result<BookDetailDto>> GetByIdAsync(
      Guid id,
      bool includeInactive = false,
      CancellationToken ct = default
   );

   Task<Result<IEnumerable<BookListItemDto>>> SearchAsync(
      BookSearchField searchField,
      string searchText,
      bool includeInactive = false,
      CancellationToken ct = default
   );

   Task<Result<BookDto>> CreateAsync(
      BookCreateDto dto,
      CancellationToken ct = default
   );

   Task<Result<BookItemDto>> AddBookItemAsync(
      Guid bookId,
      BookItemAddDto dto,
      CancellationToken ct = default
   );

   Task<Result<BookDeactivationInfoDto>> GetDeactivationInfoAsync(
      Guid bookId,
      CancellationToken ct = default
   );

   Task<Result<BookDto>> DeactivateAsync(
      Guid bookId,
      CancellationToken ct = default
   );
}
