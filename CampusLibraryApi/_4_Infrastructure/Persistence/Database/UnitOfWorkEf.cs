using CampusLibraryApi._2_Shared._1_Ports;
namespace CampusLibraryApi._4_Infrastructure.Persistence.Database;

internal sealed class UnitOfWorkEf(
   LibraryDbContext dbContext
) : IUnitOfWork {
   public Task<int> SaveChangesAsync(CancellationToken ct)
      => dbContext.SaveChangesAsync(ct);
}
