using System.Data.Common;
using CampusLibraryApi;
using CampusLibraryApi._2_BuildingBlocks._1_Ports;
using CampusLibraryApi._4_Infrastructure.Persistence.Database;
using CampusLibraryApiTest.TestInfrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CampusLibraryApiTest.TestController;

/// <summary>
/// Integration-test host for CampusLibraryApi using a test SQLite database.
/// </summary>
public sealed class TestBaseFactory : WebApplicationFactory<Program> {
   private readonly DbMode _dbMode;
   private readonly string _databaseName;
   private readonly bool _applyMigrations;
   private readonly bool _enableSensitiveDataLogging;
   private readonly bool _deleteDatabaseOnDispose;

   private string _dbPath = string.Empty;
   private DbConnection? _dbConnection;

   public DateTime TestCreatedAt { get; set; } =
      new(2025, 01, 01, 00, 00, 00, DateTimeKind.Utc);

   public TestBaseFactory(
      DbMode dbMode,
      string databaseName = "ApiTest",
      bool applyMigrations = true,
      bool enableSensitiveDataLogging = true,
      bool deleteDatabaseOnDispose = false
   ) {
      _dbMode = dbMode;
      _databaseName = databaseName;
      _applyMigrations = applyMigrations;
      _enableSensitiveDataLogging = enableSensitiveDataLogging;
      _deleteDatabaseOnDispose = deleteDatabaseOnDispose;
   }

   public async Task InitializeAsync() {
      var (dbPath, dbConnection, dbContext) = await TestDatabase.CreateAsync(
         mode: _dbMode,
         databaseName: _databaseName,
         applyMigrations: _applyMigrations,
         enableSensitiveDataLogging: _enableSensitiveDataLogging
      );

      _dbPath = dbPath;
      _dbConnection = dbConnection;
      await dbContext.DisposeAsync();
   }

   public override async ValueTask DisposeAsync() {
      await TestDatabase.DisposeAsync(
         mode: _dbMode,
         dbPath: _dbPath,
         dbConnection: _dbConnection,
         dbContext: null,
         deleteDatabaseFile: _deleteDatabaseOnDispose
      );

      await base.DisposeAsync();
   }

   protected override void ConfigureWebHost(IWebHostBuilder builder) {
      builder.ConfigureAppConfiguration((_, config) => {
         config.AddJsonFile(
            path: Path.Combine(AppContext.BaseDirectory, "appsettingsTest.json"),
            optional: false,
            reloadOnChange: false
         );
      });

      builder.ConfigureServices(services => {
         if(_dbConnection is null)
            throw new InvalidOperationException(
               "Factory not initialized. Did you call InitializeAsync()?"
            );

         services.RemoveAll<DbContextOptions<AppDbContext>>();
         services.RemoveAll<AppDbContext>();
         services.RemoveAll<IDbContextFactory<AppDbContext>>();

         services.AddDbContext<AppDbContext>(options => {
            options.UseSqlite(_dbConnection);
            if(_enableSensitiveDataLogging)
               options.EnableSensitiveDataLogging();
         });

         services.RemoveAll<IUnitOfWork>();
         services.AddScoped<IUnitOfWork, UnitOfWorkEf>();

         services.RemoveAll<IClock>();
         services.AddSingleton<IClock>(new FakeClock(TestCreatedAt));

         services.AddScoped<TestSeed>();
      });
   }

   public string DatabasePath => _dbPath;

   public IServiceScope CreateScope() => Services.CreateScope();

   public async Task WithScopeAsync(Func<IServiceProvider, Task> action) {
      using var scope = CreateScope();
      await action(scope.ServiceProvider);
   }
}
