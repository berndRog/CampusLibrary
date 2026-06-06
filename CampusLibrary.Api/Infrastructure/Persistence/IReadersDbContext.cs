using CampusLibrary.Api.Readers.Domain;
using Microsoft.EntityFrameworkCore;

namespace CampusLibrary.Api.Infrastructure.Persistence;

public interface IReadersDbContext {
   DbSet<Reader> Readers { get; }
   Task<int> SaveChangesAsync(CancellationToken ct);
}
