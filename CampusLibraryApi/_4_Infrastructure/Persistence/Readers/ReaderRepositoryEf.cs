using CampusLibraryApi._3_Core.Readers._1_Ports;
using CampusLibraryApi._3_Core.Readers._3_Domain.Entities;
using CampusLibraryApi._3_Core.Readers._3_Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
namespace CampusLibraryApi._4_Infrastructure.Persistence.Repositories;

internal sealed class ReaderRepositoryEf(
   IReaderDbContext dbContext
) : IReaderRepository {
   
   public async Task<Reader?> FindByIdAsync(
      Guid id, 
      CancellationToken ct
   ) =>  await dbContext.Readers
            .FirstOrDefaultAsync(r => r.Id == id, ct);
   
   public async Task<Reader?> FindBySubjectAsync(
      string subject, 
      CancellationToken ct
   ) => await dbContext.Readers
            .FirstOrDefaultAsync(r => r.Subject == subject, ct);

   public async Task<Reader?> FindByEmailAsync(
      EmailVo emailVo, 
      CancellationToken ct
   ) => await dbContext.Readers
      .FirstOrDefaultAsync(r => r.EmailVo == emailVo, ct);


   public async Task<bool> ExistsBySubjectAsync(
      string subject, 
      CancellationToken ct
   ) => await dbContext.Readers
         .AnyAsync(r => r.Subject == subject, ct);
   
   public void Add(Reader reader) => 
      dbContext.Add(reader);
   
   public void AddRange(IEnumerable<Reader> readers) =>
       dbContext.AddRange(readers);

}
