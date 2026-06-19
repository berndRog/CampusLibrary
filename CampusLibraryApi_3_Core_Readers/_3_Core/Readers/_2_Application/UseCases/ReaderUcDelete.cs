using CampusLibraryApi._2_BuildingBlocks;
using CampusLibraryApi._2_BuildingBlocks._1_Ports;
using CampusLibraryApi._3_Core.Readers._1_Ports;
using CampusLibraryApi._3_Core.Readers._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Readers._3_Domain.Errors;
using Microsoft.Extensions.Logging;

namespace CampusLibraryApi._3_Core.Readers._2_Application.UseCases;

// Use case for deleting a Reader aggregate.
// Coordinates aggregate loading, repository removal and persistence.
// This is a command-side application service and therefore returns a Result.
public sealed class ReaderUcDelete(
   IReaderRepository repository,
   IUnitOfWork unitOfWork,
   ILogger<ReaderUcDelete> logger
) {
   // Execute the delete-reader workflow.
   public async Task<Result> ExecuteAsync(
      Guid id,
      CancellationToken ct
   ) {
      if (id == Guid.Empty)
         return Result.Failure(ReaderErrors.InvalidId);

      var reader = await repository.FindByIdAsync(id, ct);
      if (reader is null)
         return Result.Failure(ReaderErrors.ReaderNotFound);

      repository.Remove(reader);

      var rows = await unitOfWork.SaveAllChangesAsync("ReaderUcDelete", ct);

      logger.LogInformation("ReaderUcDelete: readerId={ReaderId} rows={Rows}",
         reader.Id, rows);

      return Result.Success();
   }
}

/*
Didaktik
--------

ReaderUcDelete ist ein schreibender Use Case.

Der Use Case lädt zuerst das Reader-Aggregate. Nur wenn der Reader existiert,
wird er über das Repository aus dem aktuellen Unit-of-Work-Kontext entfernt.
Das eigentliche Löschen in der Datenbank passiert erst durch SaveChanges in
der UnitOfWork.

In Schritt 1 wird ein Reader physisch gelöscht. Eine spätere Regel wie
"Reader mit aktiven Loans darf nicht gelöscht werden" gehört in einen
späteren Ausbauschritt, sobald das Loans-Modul existiert.

Ablauf:

1. Id prüfen
2. Reader-Aggregate laden
3. NotFound zurückgeben, wenn kein Reader existiert
4. Repository.Remove(...) aufrufen
5. UnitOfWork zum Speichern verwenden
6. Success ohne Rückgabewert liefern

Lernziele
---------

- Delete als schreibenden Use Case verstehen
- NotFound vor dem Löschen explizit prüfen
- Repository.Remove(...) vom tatsächlichen SaveChanges unterscheiden
- zukünftige fachliche Löschregeln vorbereiten
*/
