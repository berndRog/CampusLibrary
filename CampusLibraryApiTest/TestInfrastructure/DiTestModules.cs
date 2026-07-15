using System.Data.Common;
using CampusLibraryApi._2_BuildingBlocks._1_Ports;
using CampusLibraryApi._2_BuildingBlocks._1_Ports.Contracts;
using CampusLibraryApi._3_Core.Catalog._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Catalog._2_Application.UseCases;
using CampusLibraryApi._3_Core.Loans._1_Ports.Inbound;
using CampusLibraryApi._3_Core.Loans._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Loans._2_Application.UseCases;
using CampusLibraryApi._3_Core.Readers._1_Ports;
using CampusLibraryApi._3_Core.Readers._1_Ports.Inbound;
using CampusLibraryApi._3_Core.Readers._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Readers._2_Application.UseCases;
using CampusLibraryApi._4_Infrastructure.Persistence;
using CampusLibraryApi._4_Infrastructure.Persistence.Catalog;
using CampusLibraryApi._4_Infrastructure.Persistence.Contracts;
using CampusLibraryApi._4_Infrastructure.Persistence.Database;
using CampusLibraryApi._4_Infrastructure.Persistence.Loans;
using CampusLibraryApi._4_Infrastructure.Persistence.Readers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
namespace CampusLibraryApiTest.TestInfrastructure;

public static class DiTestModules {

   public static IServiceCollection AddTestModules(
      this IServiceCollection services,
      DbConnection dbConnection,
      bool enableSensitiveDataLogging = true
   ) {
      services.AddSingleton(dbConnection);

      services.AddDbContext<AppDbContext>((sp, options) => {
         var connection = sp.GetRequiredService<DbConnection>();
         options.UseSqlite(connection);

         if (enableSensitiveDataLogging)
            options.EnableSensitiveDataLogging();
      });

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

      // Reader UseCases
      services.AddScoped<IReaderUseCases, ReaderUseCases>();
      services.AddScoped<ReaderUcCreate>();
      services.AddScoped<ReaderUcUpdate>();
      services.AddScoped<ReaderUcDeactivate>();

      services.AddScoped<BookUcCreate>();
      services.AddScoped<BookUcAddBookItem>();
      services.AddScoped<BookUcDeactivate>();
      services.AddScoped<IBookUseCases, BookUseCases>();

      services.AddScoped<LoanUcBorrow>();
      services.AddScoped<LoanUcRenew>();
      services.AddScoped<LoanUcReturnAtDesk>();
      services.AddScoped<ILoanUseCases, LoanUseCases>();

      // Unit of Work
      services.AddScoped<IUnitOfWork, UnitOfWorkEf>();
      // Clock
      services.AddSingleton<IClock>(_ => new FakeClock(FakeClock.DefaultUtcNow));

      // Part 5 simulates the technical identity without an IA server.
      // Use cases and read models still depend only on IIdentityGateway.
      services.AddScoped<IIdentityGateway>(_ => new FakeIdentityGateway());

      // Seed
      services.AddScoped<Seed>();
      services.AddScoped<TestSeed>();

      return services;
   }
}
