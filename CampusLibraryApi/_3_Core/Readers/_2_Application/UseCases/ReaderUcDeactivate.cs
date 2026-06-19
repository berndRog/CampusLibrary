using System.Runtime.CompilerServices;
using CampusLibraryApi._2_BuildingBlocks;
using CampusLibraryApi._2_BuildingBlocks._1_Ports;
using CampusLibraryApi._3_Core.Readers._1_Ports;
using CampusLibraryApi._3_Core.Readers._3_Domain.Errors;

[assembly: InternalsVisibleTo("CampusLibraryApiTest")]
// when mocking the logger, this is need
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2, PublicKey=0024000004800000940000000602000000240000525341310004000001000100c547cac37abd99c8db225ef2f6c8a3602f3b3606cc9891605d02baa56104f4cfc0734aa39b93bf7852f7d9266654753cc297e7d2edfe0bac1cdcf9f717241550e0a7b191195b7667bb4f64bcb8e2121380fd1d9d46ad2d92d2d15605093924cceaf74c4861eff62abf69b9291ed0a340e113be11e6a7d3113e92484cf7045cc7")]
namespace CampusLibraryApi._3_Core.Readers._2_Application.UseCases;

// Use case for deactivating a Reader aggregate.
// Deactivation is a soft delete: the Reader remains stored,
// but disappears from normal read model queries.
internal sealed class ReaderUcDeactivate(
   IReaderRepository repository,
   IUnitOfWork unitOfWork,
   IClock clock,
   ILogger<ReaderUcDeactivate> logger
) {

   public async Task<Result> ExecuteAsync(
      Guid id,
      CancellationToken ct
   ) {   
      // guards
      if (id == Guid.Empty)
         return Result.Failure(ReaderErrors.InvalidId);
      
      var reader = await repository.FindByIdAsync(id, ct);
      if (reader is null)
         return Result.Failure(ReaderErrors.ReaderNotFound);

      var result = reader.Deactivate(updatedAt: clock.UtcNow);
      if (result.IsFailure)
         return result;

      var rows = await unitOfWork.SaveAllChangesAsync("ReaderUcDeactivate", ct);

      logger.LogInformation("ReaderUcDeactivate {Id}, rows: {Rows} ",
         reader.Id, rows);
      
      return Result.Success();
   }
}

/*
Didaktik
--------

ReaderUcDeactivate koordiniert das fachliche Deaktivieren eines Readers.

Der UseCase entscheidet nicht selbst, ob ein Reader deaktiviert werden darf.
Diese fachliche Regel gehört in das Aggregate Reader.

Deshalb ruft der UseCase reader.Deactivate(...) auf und wertet das
zurückgegebene Result aus.

Mögliche Ergebnisse:

- Reader existiert nicht:
  Der UseCase gibt ReaderErrors.ReaderNotFound zurück.

- Reader ist bereits deaktiviert:
  Die Domain-Methode gibt ReaderErrors.IsAlreadyDeactivated zurück.

- Reader ist aktiv:
  Die Domain setzt IsActive auf false, aktualisiert UpdatedAt und der
  UseCase speichert die Änderung über UnitOfWork.

Wichtig ist: Nur bei erfolgreicher fachlicher Änderung wird gespeichert.

Lernziele
---------

- Fachliche Regeln im Aggregate kapseln
- UseCases als Koordinatoren verstehen
- Result für erwartbare fachliche Fehler verwenden
- Speichern nur nach erfolgreicher Domain-Operation ausführen
- Soft Delete als fachliche Deaktivierung modellieren
*/