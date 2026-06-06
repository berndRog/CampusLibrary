using CampusLibrary.Api.Infrastructure.Persistence;
using CampusLibrary.Api.Readers.Application.Ports;
using CampusLibrary.Api.Readers.Domain;
using Microsoft.EntityFrameworkCore;

namespace CampusLibrary.Api.Infrastructure.Readers;

internal sealed class ReaderRepositoryEf(
   IReadersDbContext dbContext
) : IReaderRepository {

   public async Task<bool> ExistsBySubjectAsync(string subject, CancellationToken ct) {
      return await dbContext.Readers.AnyAsync(r => r.Subject == subject, ct);
   }

   public async Task InsertAsync(Reader reader, CancellationToken ct) {
      await dbContext.Readers.AddAsync(reader, ct);
   }

   public async Task<Reader?> FindByIdAsync(Guid id, CancellationToken ct) {
      return await dbContext.Readers.FirstOrDefaultAsync(r => r.Id == id, ct);
   }

   public async Task<Reader?> FindBySubjectAsync(string subject, CancellationToken ct) {
      return await dbContext.Readers.FirstOrDefaultAsync(r => r.Subject == subject, ct);
   }
}
