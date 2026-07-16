using System.Runtime.CompilerServices;
using CampusLibraryApi._2_BuildingBlocks;
using CampusLibraryApi._2_BuildingBlocks._1_Ports;
using CampusLibraryApi._3_Core.Loans._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Loans._3_Domain.Errors;
using Microsoft.Extensions.Logging;

[assembly: InternalsVisibleTo("CampusLibraryApiTest")]
namespace CampusLibraryApi._3_Core.Loans._2_Application.UseCases;

internal sealed class LoanUcReturnAtDesk(
   ILoanRepository loanRepository,
   IUnitOfWork unitOfWork,
   ILogger<LoanUcReturnAtDesk> logger
) {
   public async Task<Result> ExecuteAsync(
      Guid loanId,
      CancellationToken ct = default
   ) {
      if(loanId == Guid.Empty)
         return Result.Failure(LoanErrors.LoanIdRequired);

      var loan = await loanRepository.FindByIdAsync(loanId, ct);
      if(loan is null)
         return Result.Failure(LoanErrors.LoanNotFound);

      loanRepository.Remove(loan);

      var rows = await unitOfWork.SaveAllChangesAsync("LoanUcReturnAtDesk", ct);
      logger.LogDebug(
         "LoanUcReturnAtDesk deleted loan {LoanId}, rows {Rows}",
         loan.Id,
         rows
      );

      return Result.Success();
   }
}

/*
Lernziele und Didaktik
----------------------

Dieser Use Case beschreibt die Rückgabe eines ausgeliehenen Exemplars am
Service Desk.

Der Use Case lädt ein Loan-Aggregate über das Repository. Danach wird die
fachliche Änderung nicht direkt an Properties vorgenommen, sondern über die
Domain-Methode ReturnAtDesk.

Die Domäne entscheidet, ob die Rückgabe erlaubt ist. Zum Beispiel darf eine
bereits zurückgegebene Ausleihe nicht noch einmal zurückgegeben werden.

Der Rückgabezeitpunkt wird nicht vom Client geliefert. Er kommt aus IClock.
Dadurch bleibt die Anwendung testbar und die Zeitlogik liegt nicht im
Controller.

Gespeichert wird erst am Ende über IUnitOfWork. Das Repository lädt und
verwaltet Aggregate, die Transaktion wird aber vom UnitOfWork abgeschlossen.
*/