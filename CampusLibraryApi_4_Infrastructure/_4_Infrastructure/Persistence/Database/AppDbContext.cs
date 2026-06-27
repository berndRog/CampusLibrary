using CampusLibraryApi._3_Core.Readers._3_Domain.Entities;
using CampusLibraryApi._4_Infrastructure.Persistence.Converters;
using CampusLibraryApi._4_Infrastructure.Persistence.Readers;
using Microsoft.EntityFrameworkCore;
namespace CampusLibraryApi._4_Infrastructure.Persistence.Database;

public sealed class AppDbContext(
   DbContextOptions<AppDbContext> options
) : DbContext(options) {

   public DbSet<Reader> Readers => Set<Reader>();
   
   protected override void OnModelCreating(ModelBuilder modelBuilder) {
      
      // DateTime Converter UTC
      var utcDtConv = new UtcDateTimeConverter();
      
      modelBuilder.ApplyConfiguration(new ConfigReader(utcDtConv));
      base.OnModelCreating(modelBuilder);
   }
}
