using CampusLibraryApi._3_Core.Readers._1_Ports;
using CampusLibraryApi._3_Core.Readers._3_Domain.Entities;
using CampusLibraryApi._4_Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;
namespace CampusLibraryApi._4_Infrastructure.Persistence.Database;

public sealed class AppDbContext(
   DbContextOptions<AppDbContext> options
) : DbContext(options) {

   public DbSet<Reader> Readers => Set<Reader>();

   protected override void OnModelCreating(ModelBuilder modelBuilder) {
      modelBuilder.ApplyConfiguration(new ConfigReader());
      base.OnModelCreating(modelBuilder);
   }
}
