using CampusLibraryApi._3_Core.Readers._3_Domain.Entities;
using Microsoft.EntityFrameworkCore;
namespace CampusLibraryApi._3_Core.Readers._1_Ports;

public interface IReadersDbContext {
   DbSet<Reader> Readers { get; }
   Task<int> SaveChangesAsync(CancellationToken ct);
}
