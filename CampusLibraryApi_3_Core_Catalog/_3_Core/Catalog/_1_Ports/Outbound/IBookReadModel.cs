using CampusLibraryApi._3_Core.Catalog._2_Application.Dtos;
using CampusLibraryApi._3_Core.Catalog._2_Application.Enums;
using CampusLibraryApi._2_BuildingBlocks;

namespace CampusLibraryApi._3_Core.Catalog._1_Ports.Outbound;

// Query-side port for Book resources. List, search and detail operations use
// the same public BookDto so the API contract remains small and predictable.
public interface IBookReadModel {

   Task<Result<BookDto>> FindByIdAsync(
      Guid id,
      bool includeInactive = false,
      CancellationToken ct = default
   );

   Task<Result<BookDeactivationInfoDto>> FindDeactivationInfoAsync(
      Guid id,
      CancellationToken ct = default
   );

   Task<Result<IReadOnlyList<BookDto>>> SelectAllAsync(
      bool includeInactive = false,
      CancellationToken ct = default
   );

   // The two HTTP query values are passed directly. A separate DTO would only
   // repeat the route/query parameters without adding a fachliche meaning.
   Task<Result<IReadOnlyList<BookDto>>> SearchAsync(
      BookSearchField searchField,
      string searchText,
      bool includeInactive = false,
      CancellationToken ct = default
   );
}
