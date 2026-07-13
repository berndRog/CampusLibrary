using System.Runtime.CompilerServices;
using CampusLibraryApi._2_BuildingBlocks;
using CampusLibraryApi._2_BuildingBlocks._1_Ports;
using CampusLibraryApi._3_Core.Readers._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Readers._3_Domain.Errors;
using Microsoft.Extensions.Logging;

[assembly: InternalsVisibleTo("CampusLibraryApiTest")]
// when mocking the logger, this is need
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2, PublicKey=0024000004800000940000000602000000240000525341310004000001000100c547cac37abd99c8db225ef2f6c8a3602f3b3606cc9891605d02baa56104f4cfc0734aa39b93bf7852f7d9266654753cc297e7d2edfe0bac1cdcf9f717241550e0a7b191195b7667bb4f64bcb8e2121380fd1d9d46ad2d92d2d15605093924cceaf74c4861eff62abf69b9291ed0a340e113be11e6a7d3113e92484cf7045cc7")]
namespace CampusLibraryApi._3_Core.Readers._2_Application.UseCases;

// Use case for deactivating a Reader aggregate.
// Deactivation is a soft delete: the Reader remains stored,
// but disappears from normal read model queries.
internal sealed class ReaderUcDeactivate(
   IReaderRepository repository,
   ILoanReaderContract loanReaderContract,
   IUnitOfWork unitOfWork,
   IClock clock,
   ILogger<ReaderUcDeactivate> logger
) {

   public async Task<Result> ExecuteAsync(
      Guid id,
      CancellationToken ct
   ) {
      // guards
      if(id == Guid.Empty)
         return Result.Failure(ReaderErrors.InvalidId);

      var reader = await repository.FindByIdAsync(id, ct);
      if(reader is null)
         return Result.Failure(ReaderErrors.ReaderNotFound);

      // The Reader aggregate cannot know whether Loans contains current
      // borrowings. This cross-module rule is therefore coordinated here.
      bool hasCurrentLoans = await loanReaderContract.ExistsForReaderAsync(
         readerId: reader.Id,
         ct: ct
      );

      if(hasCurrentLoans)
         return Result.Failure(
            ReaderErrors.ReaderCannotBeDeactivatedWithLoans
         );

      var result = reader.Deactivate(
         updatedAt: clock.UtcNow
      );

      if(result.IsFailure)
         return result;

      var rows = await unitOfWork.SaveAllChangesAsync(
         "ReaderUcDeactivate",
         ct
      );

      logger.LogInformation(
         "ReaderUcDeactivate {Id}, rows: {Rows}",
         reader.Id,
         rows
      );

      return Result.Success();
   }
}

/*
Didaktik
--------

ReaderUcDeactivate koordiniert das fachliche Deaktivieren eines Readers.

Zwei unterschiedliche Arten von Regeln werden sichtbar:

1. Regel innerhalb des Reader-Aggregates
   Der Reader weiß selbst, ob er bereits deaktiviert ist. Deshalb liegt diese
   Prüfung in Reader.Deactivate(...).

2. Regel über mehrere Module
   Ob aktuelle Ausleihen existieren, kann das Reader-Aggregate nicht wissen.
   Die Loans gehören dem Loans-Modul. Der UseCase fragt deshalb über
   ILoanReaderContract nach, ohne direkt auf die Loans-Tabelle zuzugreifen.

Mögliche Ergebnisse:

- Reader existiert nicht:
  Der UseCase gibt ReaderErrors.ReaderNotFound zurück.

- Aktuelle Ausleihen existieren:
  Der UseCase gibt ReaderErrors.ReaderCannotBeDeactivatedWithLoans zurück.
  Der HTTP-Controller übersetzt diesen Conflict in Statuscode 409.

- Reader ist bereits deaktiviert:
  Die Domain-Methode gibt ReaderErrors.IsAlreadyDeactivated zurück.

- Reader ist aktiv und besitzt keine aktuellen Ausleihen:
  Die Domain setzt IsActive auf false, aktualisiert UpdatedAt und der UseCase
  speichert die Änderung über UnitOfWork.

Wichtig ist: Nur bei erfolgreicher fachlicher Änderung wird gespeichert.

Lernziele
---------

- Regeln innerhalb eines Aggregates von modulübergreifenden Regeln trennen
- UseCases als Koordinatoren verstehen
- Modulübergreifende Abfragen über kleine Contracts durchführen
- Result für erwartbare fachliche Fehler verwenden
- HTTP 409 Conflict für einen fachlich momentan unzulässigen Zustand verwenden
- Speichern nur nach erfolgreicher Domain-Operation ausführen
- Soft Delete als fachliche Deaktivierung modellieren
*/
