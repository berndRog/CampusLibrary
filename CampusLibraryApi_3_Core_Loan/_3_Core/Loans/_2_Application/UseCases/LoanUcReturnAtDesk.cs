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

Der Use Case lädt den aktuellen Loan über das Repository und entfernt ihn.
Damit ist die Rückgabe durch die Abwesenheit des Loan-Datensatzes modelliert.
Ein weiterer Rückgabeversuch liefert folgerichtig LoanNotFound.

Gespeichert wird erst am Ende über IUnitOfWork. Das Repository verwaltet das
Aggregate, die Transaktion wird durch UnitOfWork abgeschlossen.
*/
