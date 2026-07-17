using CampusLibraryApi._3_Core.Catalog._3_Domain.Entities;

namespace CampusLibraryApi._3_Core.Catalog._1_Ports.Outbound;

public interface IBookRepository {

   Task<Book?> FindByIdAsync(
      Guid id,
      CancellationToken ct = default
   );

   Task<bool> ExistsByIsbnAsync(
      string isbn,
      CancellationToken ct = default
   );
   
   void Add(Book book);
   void AddRange(IEnumerable<Book> books);
   
}