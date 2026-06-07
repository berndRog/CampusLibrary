using CampusLibraryApi._3_Core.Readers._3_Domain.Entities;
using CampusLibraryApi._3_Core.Readers._3_Domain.ValueObjects;
namespace CampusLibraryApi._3_Core.Readers._1_Ports;

public interface IReaderRepository {
   
   Task<Reader?> FindByIdAsync(
      Guid id, 
      CancellationToken ct
   );
   
   Task<Reader?> FindBySubjectAsync(
      string subject, 
      CancellationToken ct
   );
   
   Task<Reader?> FindByEmailAsync(
      EmailVo emailVo, 
      CancellationToken ct
   );
   
   Task<bool> ExistsBySubjectAsync(
      string subject, 
      CancellationToken ct
   );
   
   void Add(Reader reader);
   
   
}
