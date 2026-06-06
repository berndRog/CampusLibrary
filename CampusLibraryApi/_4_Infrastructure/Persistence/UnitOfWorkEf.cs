using CampusLibrary.Api._2_Shared;
namespace CampusLibrary.Api._4_Infrastructure.Persistence;

internal sealed class UnitOfWorkEf(
   LibraryDbContext dbContext
) : IUnitOfWork {
   public Task<int> SaveChangesAsync(CancellationToken ct)
      => dbContext.SaveChangesAsync(ct);
}
