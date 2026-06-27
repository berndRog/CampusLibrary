using System.Runtime.CompilerServices;
using CampusLibraryApi._3_Core.Catalog._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Catalog._3_Domain.Entities;
using CampusLibraryApi._3_Core.Readers._1_Ports;
using CampusLibraryApi._3_Core.Readers._3_Domain.Entities;
using CampusLibraryApi._4_Infrastructure.Persistence.Database;
[assembly: InternalsVisibleTo("CampusLibraryApiTest")]
namespace CampusLibraryApi._4_Infrastructure.Persistence.Readers;

internal sealed class CatalogDbContextEf(
   AppDbContext db
) : ICatalogDbContext {
   
   public IQueryable<Book> Books => db.Set<Book>().AsQueryable();
   public IQueryable<BookItem> BookItems => db.Set<BookItem>().AsQueryable();
   
   public void Add(Book book) => db.Set<Book>().Add(book);

   public void AddRange(IEnumerable<Book> books)
      => db.Set<Book>().AddRange(books);
   
}