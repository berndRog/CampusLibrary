using CampusLibraryApi._2_BuildingBlocks;
using CampusLibraryApi._3_Core.Loans._2_Application.Dtos;

namespace CampusLibraryApi._3_Core.Loans._1_Ports.Outbound;

// Outbound port for read operations of the Loans module.
// The concrete implementation is provided by Infrastructure.
public interface ILoanReadModel {

   // Finds one loan by its id.
   Task<Result<LoanDto>> FindByIdAsync(
      Guid id,
      CancellationToken ct
   );

   // Returns all currently active loans.
   Task<Result<IReadOnlyList<LoanDto>>> FindAllActiveAsync(
      CancellationToken ct
   );

   // Returns all currently active loans for one reader.
   Task<Result<IReadOnlyList<LoanDto>>> FindActiveByReaderIdAsync(
      Guid readerId,
      CancellationToken ct
   );

   // Returns all active loans whose due date is before the given timestamp.
   Task<Result<IReadOnlyList<LoanDto>>> FindAllOverdueAsync(
      DateTime utcNow,
      CancellationToken ct
   );
}

/*
Lernziele und Didaktik
----------------------

Dieses Interface beschreibt die Leseseite des Loans-Moduls.

In diesem Projekt werden ReadModels als Outbound-Ports eingeordnet, weil die
Interfaces im Core definiert und die technischen Implementierungen in
Infrastructure bereitgestellt werden.

ReadModels dürfen für Abfragen optimierte Projektionen liefern. Sie müssen
nicht zwingend vollständige Aggregate laden. Dadurch wird sichtbar, dass
Lesen und Schreiben unterschiedliche Anforderungen haben können.

Die Use Cases verändern den Zustand der Domäne. Das ReadModel liefert
Daten für API-Abfragen. Diese Trennung erleichtert Tests und macht die
Architektur für Studierende übersichtlicher.
*/