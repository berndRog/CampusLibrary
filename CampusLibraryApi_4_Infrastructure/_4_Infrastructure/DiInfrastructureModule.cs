using CampusLibraryApi._2_BuildingBlocks._1_Ports;
using CampusLibraryApi._3_Core.Catalog._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Readers._1_Ports;
using CampusLibraryApi._4_Infrastructure.Persistence.Catalog;
using CampusLibraryApi._4_Infrastructure.Persistence.Database;
using CampusLibraryApi._4_Infrastructure.Persistence.Readers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
namespace CampusLibraryApi._4_Infrastructure;

public static class DiInfrastructureModule {
   public static IServiceCollection AddInfrastructureModule(
      this IServiceCollection services,
      IConfiguration configuration
   ) {
      var connectionString = configuration.GetConnectionString("CampusLibraryDb");
      Console.WriteLine("---> Using SQLite connection string: " + connectionString);

      services.AddDbContext<AppDbContext>(options =>
         options.UseSqlite(connectionString)
      );

      // BC Db Contexts
      services.AddScoped<IReaderDbContext, ReaderDbContextEf>();
      services.AddScoped<ICatalogDbContext, CatalogDbContextEf>();

      // Adapters

      // Repositories
      services.AddScoped<IReaderRepository, ReaderRepositoryEf>();
      services.AddScoped<IBookRepository, BookRepositoryEf>();
      
      // ReadModels
      services.AddScoped<IReaderReadModel, ReaderReadModelEf>();
      services.AddScoped<IBookReadModel, BookReadModelEf>();

      // Unit of Work
      services.AddScoped<IUnitOfWork, UnitOfWorkEf>();

      // // IdentityGateway
      // //services.AddScoped<IIdentityGateway, IdentityGatewayHttpContext>();
      // services.AddScoped<IIdentityGateway>(_ => new FakeIdentityGateway(
      //    subject: "11111111-0002-0000-0000-000000000000",
      //    username: "w.wagner@banking.de",
      //    createdAt: DateTime.UtcNow,
      //    adminRights: 511
      // ));

      // IClock
      services.AddScoped<IClock, AppSystemClock>();

      return services;
   }
}