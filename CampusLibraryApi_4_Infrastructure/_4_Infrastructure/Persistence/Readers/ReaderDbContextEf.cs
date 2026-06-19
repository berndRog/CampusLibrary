using System.Runtime.CompilerServices;
using CampusLibraryApi._3_Core.Readers._1_Ports;
using CampusLibraryApi._3_Core.Readers._3_Domain.Entities;
using CampusLibraryApi._4_Infrastructure.Persistence.Database;
[assembly: InternalsVisibleTo("CampusLibraryApiTest")]
namespace CampusLibraryApi._4_Infrastructure.Persistence.Readers;

internal sealed class ReaderDbContextEf(
   AppDbContext db
) : IReaderDbContext {
   
   public IQueryable<Reader> Readers
      => db.Set<Reader>();

   public void Add(Reader reader)
      => db.Set<Reader>().Add(reader);

   public void AddRange(IEnumerable<Reader> readers)
      => db.Set<Reader>().AddRange(readers);
}