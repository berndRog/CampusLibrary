using CampusLibrary.Api._3_Core.Readers.Application.Ports;
using CampusLibrary.Api._3_Core.Readers.Domain;
using CampusLibrary.Api._4_Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
namespace CampusLibrary.Api._4_Infrastructure.Readers;

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
