using System.Data.Common;
using CampusLibraryApi._2_BuildingBlocks._1_Ports;
using CampusLibraryApi._3_Core.Catalog._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Catalog._2_Application.UseCases;
using CampusLibraryApi._3_Core.Readers._1_Ports;
using CampusLibraryApi._3_Core.Readers._1_Ports.Inbound;
using CampusLibraryApi._3_Core.Readers._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Readers._2_Application.UseCases;
using CampusLibraryApi._4_Infrastructure.Persistence;
using CampusLibraryApi._4_Infrastructure.Persistence.Catalog;
using CampusLibraryApi._4_Infrastructure.Persistence.Database;
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

      // Adapters

      // Repositories
      services.AddScoped<IReaderRepository, ReaderRepositoryEf>();
      services.AddScoped<IBookRepository, BookRepositoryEf>();
      services.AddScoped<IAuthorRepository, AuthorRepositoryEf>();
      
      // ReadModels
      services.AddScoped<IReaderReadModel, ReaderReadModelEf>();
      services.AddScoped<IBookReadModel, BookReadModelEf>();
      services.AddScoped<IAuthorReadModel, AuthorReadModelEf>();

      
      // Readmodels
      services.AddScoped<IReaderReadModel, ReaderReadModelEf>();

      // Repositories
      services.AddScoped<IReaderRepository, ReaderRepositoryEf>();

      // Reader UseCases
      services.AddScoped<IReaderUseCases, ReaderUseCases>();
      services.AddScoped<ReaderUcCreate>();
      services.AddScoped<ReaderUcUpdate>();
      services.AddScoped<ReaderUcDelete>();
      
      services.AddScoped<AuthorUcCreate>();
      services.AddScoped<AuthorUcDeactivate>();
      services.AddScoped<IAuthorUseCases, AuthorUseCases>();

      services.AddScoped<BookUcCreate>();
      services.AddScoped<BookUcAddBookItem>();
      services.AddScoped<BookUcAssignAuthor>();
      services.AddScoped<BookUcDeactivate>();
      services.AddScoped<IBookUseCases, BookUseCases>();
      
      // Unit of Work
      services.AddScoped<IUnitOfWork, UnitOfWorkEf>();
      // Clock 
      services.AddSingleton<IClock>(_ => new FakeClock(FakeClock.DefaultUtcNow));

      // // IdentityGateway = CustomerRegister() from Seed
      // // simulate loggedin customer
      // services.AddScoped<IIdentityGateway>(_ => new FakeIdentityGateway{
      //    Subject = "70000000-0007-0000-0000-000000000000",
      //    Username = "e.engel@freenet.de",
      //    CreatedAt = FakeClock.DefaultUtcNow,
      //    AdminRights = 0
      // });

      // Seed
      services.AddScoped<Seed>();
      services.AddScoped<TestSeed>();

      return services;
   }
}
