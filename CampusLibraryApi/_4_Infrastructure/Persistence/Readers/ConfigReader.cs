using CampusLibraryApi._3_Core.Readers._3_Domain.Entities;
using CampusLibraryApi._3_Core.Readers._3_Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace CampusLibraryApi._4_Infrastructure.Persistence.Configurations;

internal sealed class ConfigReader : IEntityTypeConfiguration<Reader> {
   
   public void Configure(EntityTypeBuilder<Reader> builder) {
      
      // tablename
      builder.ToTable("Readers");
      
      // primary key
      builder.HasKey(r => r.Id);
      builder.Property(c => c.Id)
         .ValueGeneratedNever()
         .HasColumnName("Id").HasColumnOrder(0);
      
      // properties

      builder.Property(c => c.Firstname)
         .HasMaxLength(80)
         .HasColumnName("Firstname").HasColumnOrder(1)
         .IsRequired();

      builder.Property(c => c.Lastname)
         .HasMaxLength(80)
         .HasColumnName("Lastname").HasColumnOrder(2)
         .IsRequired();
      
      builder.Property(r => r.EmailVo)
         .HasConversion(
            vo => vo.Value, 
            value => EmailVo.FromPersisted(value)
         )
         .HasMaxLength(254)
         .HasColumnName("Email").HasColumnOrder(3)
         .IsRequired();
      
      // Address (owned value object)
      builder.OwnsOne(c => c.AddressVo, a => {
         a.Property(p => p.Street)
            .HasMaxLength(80)
            .HasColumnName("Street").HasColumnOrder(4)
            .IsRequired();

         a.Property(p => p.PostalCode)
            .HasMaxLength(20)
            .HasColumnName("PostalCode").HasColumnOrder(5)
            .IsRequired();

         a.Property(p => p.City)
            .HasMaxLength(80)
            .HasColumnName("City").HasColumnOrder(6)
            .IsRequired();

         a.Property(p => p.Country)
            .HasMaxLength(80)
            .HasColumnName("Country").HasColumnOrder(7)
            .IsRequired(false);
      });
      builder.Navigation(c => c.AddressVo).IsRequired();

      
      builder.Property(r => r.Subject)
         .HasMaxLength(200)
         .HasColumnName("Subject").HasColumnOrder(8)
         .IsRequired();
      builder.HasIndex(r => r.Subject).IsUnique();
      
   }
}
