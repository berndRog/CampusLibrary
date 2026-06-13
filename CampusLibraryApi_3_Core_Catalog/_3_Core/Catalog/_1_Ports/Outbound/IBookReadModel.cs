using CampusLibraryApi._3_Core.Catalog._2_Application.Dtos;

namespace CampusLibraryApi._3_Core.Catalog._1_Ports.Outbound;

public interface IBookReadModel {

   Task<IReadOnlyList<BookListItemDto>> SearchAsync(
      BookSearchDto search,
      CancellationToken ct = default
   );

   Task<IReadOnlyList<BookListItemDto>> SelectByAuthorIdAsync(
      Guid authorId,
      CancellationToken ct = default
   );
}