using CampusLibrary.Api.Readers.Domain;
using Microsoft.EntityFrameworkCore;

namespace CampusLibrary.Api.Infrastructure.Persistence;

public sealed class LibraryDbContext(
   DbContextOptions<LibraryDbContext> options
) : DbContext(options), IReadersDbContext {

   public DbSet<Reader> Readers => Set<Reader>();

   protected override void OnModelCreating(ModelBuilder modelBuilder) {
      modelBuilder.ApplyConfiguration(new ConfigReader());
      base.OnModelCreating(modelBuilder);
   }
}
