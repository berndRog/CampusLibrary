using System.Data.Common;
using CampusLibraryApi._4_Infrastructure.Persistence.Database;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CampusLibraryApiTest.TestInfrastructure;

public sealed class TestApiFactory(
   DbConnection dbConnection,
   bool enableSensitiveDataLogging = true
) : WebApplicationFactory<Program> {

   protected override void ConfigureWebHost(IWebHostBuilder builder) {
      builder.UseEnvironment("Testing");

      builder.ConfigureServices(services => {
         // Replace production AppDbContext registration with the shared SQLite test connection.
         services.RemoveAll<DbContextOptions<AppDbContext>>();
         services.RemoveAll<AppDbContext>();

         services.AddDbContext<AppDbContext>(options => {
            options.UseSqlite(dbConnection);

            if (enableSensitiveDataLogging)
               options.EnableSensitiveDataLogging();
         });
      });
   }
}
