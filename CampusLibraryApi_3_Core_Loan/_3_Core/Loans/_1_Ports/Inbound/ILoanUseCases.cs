using CampusLibraryApi._2_BuildingBlocks;
using CampusLibraryApi._3_Core.Loans._2_Application.Dtos;

namespace CampusLibraryApi._3_Core.Loans._1_Ports.Inbound;

// Inbound port for command use cases of the Loans module.
// Commands return only the changed aggregate id or success without exposing
// a second command-specific Loan DTO. HTTP responses are loaded through the
// read model afterwards.
public interface ILoanUseCases {
   Task<Result<Guid>> BorrowAsync(
      LoanCreateDto dto,
      CancellationToken ct
   );

   Task<Result<Guid>> RenewAsync(
      Guid loanId,
      CancellationToken ct
   );

   Task<Result> ReturnAtDeskAsync(
      Guid loanId,
      CancellationToken ct
   );
}

/*
Lernziele und Didaktik
----------------------

Dieses Interface ist der Inbound-Port des Loans-Moduls.

Ein Inbound-Port beschreibt, welche fachlichen Anwendungsfälle ein Modul
nach außen anbietet. Der Controller kennt nur dieses Interface und nicht die
konkrete Implementierung der Use Cases.

Die Methoden sind bewusst als Commands formuliert:

- BorrowAsync legt eine neue Ausleihe an.
- RenewAsync verlängert eine aktuell ausgeliehene Ausleihe.
- ReturnAtDeskAsync registriert die Rückgabe eines ausgeliehenen Exemplars.

Leseoperationen stehen hier nicht. Sie gehören in diesem Projekt konsequent
in ein ReadModel. Dadurch bleibt die Trennung zwischen schreibenden Use Cases
und lesenden Projektionen sichtbar.

Wichtig ist außerdem: Der Client liefert bei BorrowAsync keine Leihdauer.
Die Leihdauer ist eine fachliche Regel des Loans-Moduls.

Loans besitzen kein IsActive-Flag und keinen Rückgabestatus. Ein vorhandener
Loan ist eine aktuelle Ausleihe; ReturnAtDeskAsync löscht ihn.
*/
