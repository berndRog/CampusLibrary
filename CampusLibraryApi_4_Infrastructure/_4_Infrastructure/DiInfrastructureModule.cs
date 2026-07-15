using CampusLibraryApi._2_BuildingBlocks._1_Ports;
using CampusLibraryApi._2_BuildingBlocks._1_Ports.Contracts;
using CampusLibraryApi._3_Core.Catalog._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Loans._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Readers._1_Ports.Outbound;
using CampusLibraryApi._4_Infrastructure.Persistence.Catalog;
using CampusLibraryApi._4_Infrastructure.Persistence.Contracts;
using CampusLibraryApi._4_Infrastructure.Persistence.Database;
using CampusLibraryApi._4_Infrastructure.Persistence.Loans;
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
      services.AddScoped<ILoanDbContext, LoadDbContextEf>();

      // Adapters
      services.AddScoped<IReaderLoanContract, ReaderLoanContractEf>();
      services.AddScoped<IBookItemLoanContract, BookItemLoanContractEf>();
      services.AddScoped<ILoanCatalogContract, LoanCatalogContractEf>();
      services.AddScoped<ILoanReaderContract, LoanReaderContractEf>();

      // Repositories
      services.AddScoped<IReaderRepository, ReaderRepositoryEf>();
      services.AddScoped<IBookRepository, BookRepositoryEf>();
      services.AddScoped<ILoanRepository, LoanRepositoryEf>();

      // ReadModels
      services.AddScoped<IReaderReadModel, ReaderReadModelEf>();
      services.AddScoped<IBookReadModel, BookReadModelEf>();
      services.AddScoped<ILoanReadModel, LoanReadModelEf>();

      // Unit of Work
      services.AddScoped<IUnitOfWork, UnitOfWorkEf>();

      // The technical identity adapter is registered in the Web project.
      // Infrastructure only consumes the IIdentityGateway port indirectly.

      // IClock
      services.AddScoped<IClock, AppSystemClock>();

      return services;
   }
}
