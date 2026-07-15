using System.Runtime.CompilerServices;
using CampusLibraryApi._2_BuildingBlocks;
using CampusLibraryApi._2_BuildingBlocks._1_Ports;
using CampusLibraryApi._3_Core.Loans._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Loans._3_Domain.Errors;
using CampusLibraryApi._3_Core.Loans._3_Domain.Policies;
using Microsoft.Extensions.Logging;

[assembly: InternalsVisibleTo("CampusLibraryApiTest")]
namespace CampusLibraryApi._3_Core.Loans._2_Application.UseCases;

internal sealed class LoanUcRenew(
   ILoanRepository loanRepository,
   IUnitOfWork unitOfWork,
   IClock clock,
   ILogger<LoanUcRenew> logger
) {
   public async Task<Result<Guid>> ExecuteAsync(
      Guid loanId,
      CancellationToken ct = default
   ) {
      if(loanId == Guid.Empty)
         return Result<Guid>.Failure(LoanErrors.LoanIdRequired);

      var loan = await loanRepository.FindByIdAsync(loanId, ct);
      if(loan is null)
         return Result<Guid>.Failure(LoanErrors.LoanNotFound);

      var resultRenewed = loan.Renew(
         utcNow: clock.UtcNow,
         newDueDate: loan.DueDate.AddDays(LoanRules.StandardRenewalDays)
      );
      if(resultRenewed.IsFailure)
         return Result<Guid>.Failure(resultRenewed.Error);

      var rows = await unitOfWork.SaveAllChangesAsync("LoanUcRenew", ct);
      logger.LogDebug("LoanUcRenew {LoanId} done, rows {Rows}", loan.Id, rows);

      return Result<Guid>.Success(loan.Id);
   }
}

/*
Lernziele und Didaktik
----------------------

Dieser Use Case beschreibt die Verlängerung einer aktiven Ausleihe.

Der Use Case lädt ein Loan-Aggregate über das Repository. Die fachliche
Änderung erfolgt über die Domain-Methode Renew.

Die Domäne prüft die Regeln:
- Die Ausleihe muss aktiv sein.
- Eine zurückgegebene Ausleihe darf nicht verlängert werden.
- Eine überfällige Ausleihe darf nicht verlängert werden.
- Die maximale Anzahl von Verlängerungen darf nicht überschritten werden.

Der Client liefert kein neues Rückgabedatum. Die Verlängerungsdauer ist eine
fachliche Regel des Loans-Moduls und steht in LoanRules.StandardRenewalDays.

Dadurch bleibt die Fachlogik im Anwendungskern und nicht im Controller oder
im DTO.

Gespeichert wird erst am Ende über IUnitOfWork. Das Repository lädt das
Aggregate, die Transaktion wird durch UnitOfWork abgeschlossen.
*/