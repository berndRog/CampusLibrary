using CampusLibraryApi._3_Core.Catalog._3_Domain.Entities;
using CampusLibraryApi._4_Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace CampusLibraryApi._4_Infrastructure.Persistence.Catalog;

internal sealed class ConfigAuthor(
   UtcDateTimeConverter utcDtConv
) : IEntityTypeConfiguration<Author> {
   
   public void Configure(EntityTypeBuilder<Author> builder) {

      // Table
      builder.ToTable("Authors");

      // Primary key
      builder.HasKey(a => a.Id);
      builder.Property(a => a.Id)
         .ValueGeneratedNever()
         .HasColumnName("Id")
         .HasColumnOrder(0);
      
      // Properties
      builder.Property(a => a.Firstname)
         .HasMaxLength(80)
         .HasColumnName("Firstname").HasColumnOrder(1)
         .IsRequired();

      builder.Property(a => a.Lastname)
         .HasMaxLength(80)
         .HasColumnName("Lastname").HasColumnOrder(2)
         .IsRequired();
      
      // DisplayName is a calculated domain property.
      // It is not stored as a separate database column.
      builder.Ignore(a => a.DisplayName);

      // Audit timestamps
      builder.Property(a => a.CreatedAt)
         .HasConversion(utcDtConv)
         .HasColumnName("CreatedAt").HasColumnOrder(3)
         .IsRequired();

      builder.Property(a => a.UpdatedAt)
         .HasConversion(utcDtConv)
         .HasColumnName("UpdatedAt").HasColumnOrder(4)
         .IsRequired();

      builder.Property(a => a.IsActive)
         .HasColumnName("IsActive")
         .HasColumnOrder(6).IsRequired();
      
      // Simple teaching duplicate rule.
      builder.HasIndex(a => new { a.Firstname, a.Lastname })
         .IsUnique();
   }
}