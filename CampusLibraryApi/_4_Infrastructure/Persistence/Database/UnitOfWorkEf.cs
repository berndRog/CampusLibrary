using System.Runtime.CompilerServices;
using CampusLibraryApi._2_Shared._1_Ports;
[assembly: InternalsVisibleTo("CampusLibraryApiTest")]
namespace CampusLibraryApi._4_Infrastructure.Persistence.Database;

internal sealed class UnitOfWorkEf(
   AppDbContext dbContext,
   ILogger<UnitOfWorkEf> logger
) : IUnitOfWork {
   
   public async Task<int> SaveAllChangesAsync(
      string? text = null,
      CancellationToken ct = default
   ) {
      // log repos before saving to database
      dbContext.ChangeTracker.DetectChanges();
      DumpChangeTrackerToConsole(text);

      var rows = await dbContext.SaveChangesAsync(ct);

      // log repos after saving
      DumpChangeTrackerToConsole(text);
      return rows;
   }

   public void ClearChangeTracker() =>
      dbContext.ChangeTracker.Clear();

   public void LogChangeTracker(string text) {
      if (!logger.IsEnabled(LogLevel.Debug)) return;
      DumpChangeTrackerToConsole(text);
   }

   // Workaround - Logger is cutting output
   public void DumpChangeTrackerToConsole(string? text) {
      if (!logger.IsEnabled(LogLevel.Debug)) return;

      dbContext.ChangeTracker.DetectChanges();
      var output = dbContext.ChangeTracker.DebugView.LongView;

      Console.WriteLine($"{DateTime.Now:HH:mm:ss} DEBUG ChangeTracker:");
      if (text is not null) Console.WriteLine($"=== {text} ===");
      Console.WriteLine(output);
      Console.WriteLine("=== END ===");
   }
}