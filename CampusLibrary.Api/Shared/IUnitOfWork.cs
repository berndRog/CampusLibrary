namespace CampusLibrary.Api.Shared;

public interface IUnitOfWork {

   Task<int> SaveChangesAsync(CancellationToken ct);
}
