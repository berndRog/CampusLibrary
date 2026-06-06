namespace CampusLibraryApi._2_Shared._1_Ports;

public interface IUnitOfWork {

   Task<int> SaveChangesAsync(CancellationToken ct);
}
