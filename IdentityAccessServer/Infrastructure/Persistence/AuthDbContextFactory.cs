using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
namespace IdentityAccessServer.Infrastructure.Persistence;

public sealed class AuthDbContextFactory : IDesignTimeDbContextFactory<AuthDbContext> {
   public AuthDbContext CreateDbContext(string[] args) {
      var options = new DbContextOptionsBuilder<AuthDbContext>()
         .UseSqlite("Data Source=openidauth1.0.db")
         .Options;

      return new AuthDbContext(options);
   }
}