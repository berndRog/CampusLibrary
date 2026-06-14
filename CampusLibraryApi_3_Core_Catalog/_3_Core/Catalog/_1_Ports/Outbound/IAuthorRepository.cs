using CampusLibraryApi._3_Core.Catalog._3_Domain.Entities;

namespace CampusLibraryApi._3_Core.Catalog._1_Ports.Outbound;

public interface IAuthorRepository {

   Task<Author?> FindByIdAsync(
      Guid id,
      CancellationToken ct = default
   );

   Task<bool> ExistsByNameAsync(
      string firstname,
      string lastname,
      CancellationToken ct = default
   );

   void Add(Author author);
   void AddRange(IEnumerable<Author> authors);

}
