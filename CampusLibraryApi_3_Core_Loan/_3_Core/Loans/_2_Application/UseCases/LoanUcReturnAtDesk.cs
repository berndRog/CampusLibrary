using System.Runtime.CompilerServices;
using CampusLibraryApi._2_BuildingBlocks;
using CampusLibraryApi._2_BuildingBlocks._1_Ports;
using CampusLibraryApi._3_Core.Loans._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Loans._2_Application.Dtos;
using CampusLibraryApi._3_Core.Loans._2_Application.Mappings;
using CampusLibraryApi._3_Core.Loans._3_Domain.Errors;
using Microsoft.Extensions.Logging;

[assembly: InternalsVisibleTo("CampusLibraryApiTest")]
namespace CampusLibraryApi._3_Core.Loans._2_Application.UseCases;

internal sealed class LoanUcReturnAtDesk(
   ILoanRepository loanRepository,
   IUnitOfWork unitOfWork,
   IClock clock,
   ILogger<LoanUcReturnAtDesk> logger
) {

   public async Task<Result<LoanDto>> ExecuteAsync(
      Guid loanId,
      CancellationToken ct = default
   ) {
      if (loanId == Guid.Empty)
         return Result<LoanDto>.Failure(LoanErrors.LoanIdRequired);

      // Load the Loan aggregate for a command operation.
      // The repository returns a tracked aggregate.
      var loan = await loanRepository.FindByIdAsync(
         id: loanId,
         ct: ct
      );

      if (loan is null)
         return Result<LoanDto>.Failure(LoanErrors.LoanNotFound);

      // The actual return timestamp is provided by the application service.
      var resultReturned = loan.ReturnAtDesk(
         returnedAt: clock.UtcNow
      );

      if (resultReturned.IsFailure)
         return Result<LoanDto>.Failure(resultReturned.Error);

      // Save all changes to database.
      var rows = await unitOfWork.SaveAllChangesAsync(
         "LoanUcReturnAtDesk",
         ct
      );

      logger.LogDebug(
         "LoanUcReturnAtDesk {LoanId} done, rows {Rows}",
         loan.Id,
         rows
      );

      return Result<LoanDto>.Success(
         loan.ToLoanDto()
      );
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