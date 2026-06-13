using CampusLibraryApi._3_Core.Catalog._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Catalog._3_Domain.Entities;
using CampusLibraryApi._3_Core.Catalog._3_Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace CampusLibraryApi._4_Infrastructure.Persistence.Catalog;

internal sealed class BookRepositoryEf(
   ICatalogDbContext dbContext
) : IBookRepository {

   public async Task<Book?> FindByIdAsync(
      Guid id,
      CancellationToken ct = default
   ) =>
      await dbContext.Books
         .Include(b => b.BookItems)
         .Include(b => b.Authors)
         .FirstOrDefaultAsync(b => b.Id == id, ct);

   public async Task<bool> ExistsByIsbnAsync(
      string isbn,
      CancellationToken ct = default
   ) {
      var isbnVo = IsbnVo.FromPersisted(isbn);

      return await dbContext.Books
         .AnyAsync(b => b.IsbnVo == isbnVo, ct);
   }

   public void Add(Book book)
      => dbContext.Add(book);

   public void AddRange(IEnumerable<Book> books)
      => dbContext.AddRange(books);

   public void Remove(Book book)
      => dbContext.Remove(book);
}