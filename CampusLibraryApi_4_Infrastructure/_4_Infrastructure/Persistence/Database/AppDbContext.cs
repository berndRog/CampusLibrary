using CampusLibraryApi._3_Core.Catalog._3_Domain.Entities;
using CampusLibraryApi._3_Core.Readers._3_Domain.Entities;
using CampusLibraryApi._4_Infrastructure.Persistence.Catalog;
using CampusLibraryApi._4_Infrastructure.Persistence.Converters;
using CampusLibraryApi._4_Infrastructure.Persistence.Readers;
using Microsoft.EntityFrameworkCore;
namespace CampusLibraryApi._4_Infrastructure.Persistence.Database;

public sealed class AppDbContext(
   DbContextOptions<AppDbContext> options
) : DbContext(options) {

   public DbSet<Reader> Readers => Set<Reader>();
   public DbSet<Book> Books => Set<Book>();
   public DbSet<BookItem> BookItems => Set<BookItem>();

   protected override void OnModelCreating(ModelBuilder modelBuilder) {

      var utcConv = new UtcDateTimeConverter();
      
      modelBuilder.ApplyConfiguration(new ConfigReader(utcConv));
      modelBuilder.ApplyConfiguration(new ConfigBook(utcConv));
      modelBuilder.ApplyConfiguration(new ConfigBookItem());
      
      base.OnModelCreating(modelBuilder);
   }
}
