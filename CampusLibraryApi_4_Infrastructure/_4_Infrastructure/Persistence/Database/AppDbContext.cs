using CampusLibraryApi._3_Core.Catalog._3_Domain.Entities;
using CampusLibraryApi._3_Core.Readers._1_Ports;
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
   public DbSet<Author> Authors => Set<Author>();
   public DbSet<Book> Books => Set<Book>();
   public DbSet<BookItem> BookItems => Set<BookItem>();
   
   protected override void OnModelCreating(ModelBuilder modelBuilder) {
      
      // DateTime Converter UTC
      var utcDtConv = new UtcDateTimeConverter();
      var nullUtcDtConv = new NullableUtcDateTimeConverter();

      modelBuilder.ApplyConfiguration(new ConfigReader(utcDtConv));
      modelBuilder.ApplyConfiguration(new ConfigAuthor(utcDtConv));
      modelBuilder.ApplyConfiguration(new ConfigBook(utcDtConv));
      modelBuilder.ApplyConfiguration(new ConfigBookItem());
      
      base.OnModelCreating(modelBuilder);
   }
}
