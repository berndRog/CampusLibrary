using IdentityAccessServer.Infrastructure.Identity;
using IdentityAccessServer.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityAccessServer.Infrastructure.Persistence.Configurations;

public sealed class ConfigApplicationUser(
   UtcDateTimeConverter utcDtConv
) : IEntityTypeConfiguration<ApplicationUser> {
   
   public void Configure(EntityTypeBuilder<ApplicationUser> builder) {
      builder.ToTable("AspNetUsers");

      builder.Property(u => u.Id)
         .HasColumnOrder(0);

      builder.Property(u => u.AccountType)
         .HasColumnOrder(1);

      builder.Property(u => u.UserName)
         .HasColumnOrder(2);

      builder.Property(u => u.Email)
         .HasColumnOrder(3);

      builder.Property(u => u.PhoneNumber)
         .HasColumnOrder(4);

      builder.Property(u => u.AdminRights)
         .HasColumnOrder(5);

      builder.Property(u => u.MustChangePassword)
         .HasColumnOrder(6);

      builder.Property(u => u.CreatedAt)
         .HasConversion(utcDtConv)
         .HasColumnOrder(7);

      builder.Property(u => u.UpdatedAt)
         .HasConversion(utcDtConv)
         .HasColumnOrder(8);

      builder.Property(u => u.NormalizedUserName)
         .HasColumnOrder(9);

      builder.Property(u => u.NormalizedEmail)
         .HasColumnOrder(10);

      builder.Property(u => u.EmailConfirmed)
         .HasColumnOrder(11);

      builder.Property(u => u.PasswordHash)
         .HasColumnOrder(12);

      builder.Property(u => u.SecurityStamp)
         .HasColumnOrder(13);

      builder.Property(u => u.ConcurrencyStamp)
         .HasColumnOrder(14);

      builder.Property(u => u.PhoneNumberConfirmed)
         .HasColumnOrder(15);

      builder.Property(u => u.TwoFactorEnabled)
         .HasColumnOrder(16);

      builder.Property(u => u.LockoutEnd)
         .HasColumnOrder(17);

      builder.Property(u => u.LockoutEnabled)
         .HasColumnOrder(18);

      builder.Property(u => u.AccessFailedCount)
         .HasColumnOrder(19);
   }
}

