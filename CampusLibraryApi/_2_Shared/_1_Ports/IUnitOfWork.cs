namespace CampusLibrary.Api._2_Shared;

public interface IUnitOfWork {

   Task<int> SaveChangesAsync(CancellationToken ct);
}
