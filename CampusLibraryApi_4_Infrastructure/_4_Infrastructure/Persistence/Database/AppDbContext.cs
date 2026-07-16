using CampusLibraryApi._3_Core.Catalog._3_Domain.Entities;
using CampusLibraryApi._3_Core.Loans._3_Domain.Entities;
using CampusLibraryApi._3_Core.Readers._1_Ports;
using CampusLibraryApi._3_Core.Readers._3_Domain.Entities;
using CampusLibraryApi._4_Infrastructure.Persistence.Catalog;
using CampusLibraryApi._4_Infrastructure.Persistence.Converters;
using CampusLibraryApi._4_Infrastructure.Persistence.Loans;
using CampusLibraryApi._4_Infrastructure.Persistence.Readers;
using Microsoft.EntityFrameworkCore;
namespace CampusLibraryApi._4_Infrastructure.Persistence.Database;

public sealed class AppDbContext(
   DbContextOptions<AppDbContext> options
) : DbContext(options) {

   public DbSet<Reader> Readers => Set<Reader>();
   public DbSet<Book> Books => Set<Book>();
   public DbSet<BookItem> BookItems => Set<BookItem>();
   public DbSet<Loan> Loans => Set<Loan>();
   
   protected override void OnModelCreating(ModelBuilder modelBuilder) {
      
      // DateTime Converter UTC
      var utcDtConv = new UtcDateTimeConverter();
      modelBuilder.ApplyConfiguration(new ConfigReader(utcDtConv));
      modelBuilder.ApplyConfiguration(new ConfigBook(utcDtConv));
      modelBuilder.ApplyConfiguration(new ConfigBookItem());
      modelBuilder.ApplyConfiguration(new ConfigLoan(utcDtConv));
      
      base.OnModelCreating(modelBuilder);
   }
}
