using CampusLibraryApi._3_Core.Catalog._3_Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace CampusLibraryApi._4_Infrastructure.Persistence.Catalog;

internal sealed class ConfigBookItem : IEntityTypeConfiguration<BookItem> {

   public void Configure(EntityTypeBuilder<BookItem> builder) {

      // Table
      builder.ToTable("BookItems");

      // Primary key
      builder.HasKey(bi => bi.Id);
      builder.Property(bi => bi.Id)
         .ValueGeneratedNever()
         .HasColumnName("Id").HasColumnOrder(0);
      
      builder.Property(bi => bi.Status)
         .HasConversion<int>()
         .HasColumnName("Status").HasColumnOrder(1)
         .IsRequired();

      builder.Property(bi => bi.BookId)
         .HasColumnName("BookId").HasColumnOrder(2)
         .IsRequired();
   }
}