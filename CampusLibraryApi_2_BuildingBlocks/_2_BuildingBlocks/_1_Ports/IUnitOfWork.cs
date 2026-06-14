namespace CampusLibraryApi._2_BuildingBlocks._1_Ports;

public interface IUnitOfWork {
   
   // Persist all tracked changes to the database
   // Returns the number of affected rows/entities
   Task<int> SaveAllChangesAsync(
      string? text = null,
      CancellationToken ct = default
   );

   // Clears the ORM change tracker
   // Useful in tests or after manual state corrections
   void ClearChangeTracker();

   // Writes the current change tracker state to logs
   // Helpful for debugging persistence behavior
   void LogChangeTracker(string text);
}
