using CampusLibrary.Api.Shared;

namespace CampusLibrary.Api.Infrastructure.Persistence;

internal sealed class UnitOfWorkEf(
   LibraryDbContext dbContext
) : IUnitOfWork {
   public Task<int> SaveChangesAsync(CancellationToken ct)
      => dbContext.SaveChangesAsync(ct);
}
