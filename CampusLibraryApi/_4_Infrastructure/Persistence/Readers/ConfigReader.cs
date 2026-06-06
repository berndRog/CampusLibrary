using CampusLibraryApi._3_Core.Readers._3_Domain.Entities;
using CampusLibraryApi._3_Core.Readers._3_Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace CampusLibraryApi._4_Infrastructure.Persistence.Configurations;

internal sealed class ConfigReader : IEntityTypeConfiguration<Reader> {
   
   public void Configure(EntityTypeBuilder<Reader> builder) {
      builder.ToTable("Readers");
      builder.HasKey(r => r.Id);
      builder.Property(r => r.Id).ValueGeneratedNever().HasColumnName("Id").HasColumnOrder(0);
      builder.Property(r => r.Subject).HasMaxLength(200).HasColumnName("Subject").HasColumnOrder(1).IsRequired();
      builder.HasIndex(r => r.Subject).IsUnique();
      builder.Property(r => r.EmailVo)
         .HasConversion(vo => vo.Value, value => EmailVo.FromPersisted(value))
         .HasMaxLength(120)
         .HasColumnName("Email")
         .HasColumnOrder(2)
         .IsRequired();
      builder.Property(r => r.DisplayName).HasMaxLength(80).HasColumnName("DisplayName").HasColumnOrder(3).IsRequired();
   }
}
