using IdentityAccessServer.Infrastructure.Identity;
using IdentityAccessServer.Infrastructure.Persistence.Configurations;
using IdentityAccessServer.Infrastructure.Persistence.Converters;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace IdentityAccessServer.Infrastructure.Persistence;

public sealed class AuthDbContext
   : IdentityDbContext<ApplicationUser, IdentityRole, string> {
   
   public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) {
   }

   protected override void OnModelCreating(ModelBuilder builder) {
      base.OnModelCreating(builder);

      // DateTime Converter UTC
      var utcDtConv = new UtcDateTimeConverter();
      var nullUtcDtConv = new NullableUtcDateTimeConverter();

      builder.ApplyConfiguration(new ConfigApplicationUser(utcDtConv));      
      builder.ApplyConfigurationsFromAssembly(typeof(AuthDbContext).Assembly);

      // Adds OpenIddict entity mappings to the same database
      builder.UseOpenIddict();
   }
}