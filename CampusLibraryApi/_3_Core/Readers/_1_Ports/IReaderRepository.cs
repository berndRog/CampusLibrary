using CampusLibrary.Api._3_Core.Readers.Domain;
namespace CampusLibrary.Api._3_Core.Readers.Application.Ports;

public interface IReaderRepository {
   Task<bool> ExistsBySubjectAsync(string subject, CancellationToken ct);
   Task InsertAsync(Reader reader, CancellationToken ct);
   Task<Reader?> FindByIdAsync(Guid id, CancellationToken ct);
   Task<Reader?> FindBySubjectAsync(string subject, CancellationToken ct);
}
