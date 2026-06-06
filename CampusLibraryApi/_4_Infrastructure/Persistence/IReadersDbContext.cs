using CampusLibrary.Api._3_Core.Readers.Domain;
using Microsoft.EntityFrameworkCore;
namespace CampusLibrary.Api._4_Infrastructure.Persistence;

public interface IReadersDbContext {
   DbSet<Reader> Readers { get; }
   Task<int> SaveChangesAsync(CancellationToken ct);
}
