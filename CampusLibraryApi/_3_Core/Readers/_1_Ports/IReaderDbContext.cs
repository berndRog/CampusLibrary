using CampusLibraryApi._3_Core.Readers._3_Domain.Entities;
namespace CampusLibraryApi._3_Core.Readers._1_Ports;

public interface IReaderDbContext {
   
   // Query access to Customer aggregates
   IQueryable<Reader> Readers { get; }

   // Add a new entity to the persistence context
   void Add(Reader reader);
   void AddRange(IEnumerable<Reader> readers);
   
}
