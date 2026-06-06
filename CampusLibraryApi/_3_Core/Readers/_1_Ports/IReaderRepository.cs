using CampusLibraryApi._3_Core.Readers._3_Domain.Entities;
namespace CampusLibraryApi._3_Core.Readers._1_Ports;

public interface IReaderRepository {
   Task<bool> ExistsBySubjectAsync(string subject, CancellationToken ct);
   Task InsertAsync(Reader reader, CancellationToken ct);
   Task<Reader?> FindByIdAsync(Guid id, CancellationToken ct);
   Task<Reader?> FindBySubjectAsync(string subject, CancellationToken ct);
}
