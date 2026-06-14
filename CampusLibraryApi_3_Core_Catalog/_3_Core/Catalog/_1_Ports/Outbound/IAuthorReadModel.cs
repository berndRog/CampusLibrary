using CampusLibraryApi._3_Core.Catalog._2_Application.Dtos;

namespace CampusLibraryApi._3_Core.Catalog._1_Ports.Outbound;

public interface IAuthorReadModel {

   Task<AuthorDto?> FindByIdAsync(
      Guid id,
      CancellationToken ct = default
   );

   Task<IReadOnlyList<AuthorDto>> SelectAllAsync(
      CancellationToken ct = default
   );

   Task<IReadOnlyList<AuthorDto>> SearchAsync(
      string searchText,
      CancellationToken ct = default
   );
}
