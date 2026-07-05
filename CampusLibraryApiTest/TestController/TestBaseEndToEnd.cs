using CampusLibraryApiTest.TestInfrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace CampusLibraryApiTest.TestController;

/// Base class for end to end tests that need a fresh database per test.
/// Creates a new BankingApiFactory (and thus a new DB file) for every test instance.
/// xUnit creates a NEW instance of the test class per test method by default.
public abstract class TestBaseEndToEnd : IAsyncLifetime {
   protected TestBaseFactory Factory { get; private set; } = null!;
   protected HttpClient Client { get; private set; } = null!;

   // Teaching mode:
   // - Keep DB files so students can inspect them in Rider after a debug session.
   // CI mode:
   // - Set to true to delete DB files after each test.
   protected virtual bool DeleteDatabaseOnDispose => false;

   // Create a unique DB file per test (FileUnique).
   // If you want a stable path per test name, keep FileUnique and use the test method name in DatabaseName.
   protected virtual DbMode DbMode => DbMode.FileUnique;

   // Base name for DB files (timestamp will be appended for FileUnique).
   // Override to include the test name if you want even nicer traceability.
   protected virtual string DatabaseName => "AppApiTest";

   // Optional hook for end-to-end tests that need to override specific services.
   // The hook is passed to TestBaseFactory and invoked at the end of the test DI setup.
   protected virtual void ConfigureTestServices(IServiceCollection services) {
   }

   public async ValueTask InitializeAsync() {
      Factory = new TestBaseFactory(
         dbMode: DbMode,
         databaseName: DatabaseName,
         applyMigrations: true,
         enableSensitiveDataLogging: true,
         deleteDatabaseOnDispose: DeleteDatabaseOnDispose,
         configureTestServices: ConfigureTestServices
      );

      await Factory.InitializeAsync();
      Client = Factory.CreateClient();

      // Optional: log the DB path so students can open it in Rider quickly.
      Console.WriteLine($"---> Test DB Path: {Factory.DatabasePath}");
   }

   public async ValueTask DisposeAsync() {
      Client.Dispose();
      await Factory.DisposeAsync();
   }
}

/*
DEUTSCHER DIDAKTIK-BLOCK (Vorlesung / Lernziele)

Warum "pro Test"?
- Jeder Test startet mit einer frischen, leeren Datenbank.
- Das macht Tests unabhängig (keine versteckten Abhängigkeiten / Reihenfolgeeffekte).
- In Debug-Sessions können Studierende den DB-Zustand eines einzelnen Tests nachvollziehen
  (ohne Altlasten von vorherigen Tests).

Warum FileUnique?
- Es entsteht pro Test eine eigene SQLite-Datei (einzigartig).
- Rider kann diese Datei öffnen und Tabellen/Views anzeigen.
- Für CI/Parallelität ist das robust, weil Tests sich nicht gegenseitig beeinflussen.

Warum ConfigureTestServices als Hook?
- Die meisten E2E-Tests verwenden dieselbe Test-Infrastruktur.
- Einzelne Tests dürfen aber gezielt Services austauschen, ohne die gemeinsame Factory
  für alle Tests zu verändern.
- ReaderMeControllerE2eT nutzt diesen Hook, damit der UseCase die Claims aus dem
  TestAuthHandler über IdentityGatewayHttpContext lesen kann.

Wichtige Lernpunkte:
1) xUnit Lebenszyklus
   - Standardmäßig erzeugt xUnit pro Testmethode eine neue Instanz der Testklasse.
   - Dadurch kann die Base-Class pro Test eine neue Factory/DB erzeugen.

2) Realistische DB-Struktur mit Migrationen
   - Migrate() statt EnsureCreated(), damit Views und SQL aus Migrationen existieren.

3) Nachvollziehbarkeit im Unterricht
   - DeleteDatabaseOnDispose=false behält die DB-Datei nach dem Testlauf.
   - Studierende können im Rider Database Viewer nach dem Debug weiter inspizieren.

4) Gezielte DI-Overrides
   - Tests können einzelne technische Adapter austauschen.
   - Der Produktionscode bleibt unverändert.

Lernziele:
- Studierende verstehen Test-Isolation und warum "fresh DB per test" wichtig ist.
- Studierende können den DB-Zustand als Debug-Artefakt nutzen (Tabellen/Views/Rows).
- Studierende verstehen den Zusammenhang DI (Program.cs) + Test-Overrides (Factory).
*/
