using CampusLibraryApi._2_BuildingBlocks;
using CampusLibraryApi._3_Core.Loans._2_Application.Dtos;

namespace CampusLibraryApi._3_Core.Loans._1_Ports.Inbound;

// Inbound port for command use cases of the Loans module.
// Controllers call this interface to change the state of Loan aggregates.
public interface ILoanUseCases {

   // Borrows one concrete book item for one reader.
   // The loan period is determined by the domain rules, not by the client.
   Task<Result<LoanDto>> BorrowAsync(
      LoanCreateDto dto,
      CancellationToken ct
   );

   // Renews a borrowed loan if the domain rules allow it.
   // The maximum number of renewals is defined in the domain rules.
   Task<Result<LoanDto>> RenewAsync(
      Guid loanId,
      CancellationToken ct
   );

   // Returns a borrowed book item at the service desk.
   // The actual return timestamp is provided by the application service.
   Task<Result<LoanDto>> ReturnAtDeskAsync(
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

Loans besitzen kein IsActive-Flag. Der fachliche Zustand wird über
LoanStatus modelliert. Eine offene Ausleihe hat den Status Borrowed.
*/