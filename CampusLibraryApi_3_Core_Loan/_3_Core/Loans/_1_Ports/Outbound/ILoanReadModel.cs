using CampusLibraryApi._2_BuildingBlocks;
using CampusLibraryApi._3_Core.Loans._2_Application.Dtos;

namespace CampusLibraryApi._3_Core.Loans._1_Ports.Outbound;

// Outbound port for read operations of the Loans module.
// The concrete implementation is provided by Infrastructure.
public interface ILoanReadModel {

   // Finds one loan by its id.
   Task<Result<LoanDetailDto>> FindByIdAsync(
      Guid id,
      CancellationToken ct
   );

   // Returns all currently active loans.
   Task<Result<IReadOnlyList<LoanListItemDto>>> FindAllActiveAsync(
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