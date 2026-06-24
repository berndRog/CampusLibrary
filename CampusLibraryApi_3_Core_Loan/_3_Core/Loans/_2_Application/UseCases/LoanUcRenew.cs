using System.Runtime.CompilerServices;
using CampusLibraryApi._2_BuildingBlocks;
using CampusLibraryApi._2_BuildingBlocks._1_Ports;
using CampusLibraryApi._3_Core.Loans._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Loans._2_Application.Dtos;
using CampusLibraryApi._3_Core.Loans._2_Application.Mappings;
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

      var utcNow = clock.UtcNow;

      // The renewal duration is a domain rule.
      // The client does not provide the new due date.
      var newDueDate = loan.DueDate.AddDays(
         LoanRules.StandardRenewalDays
      );

      var resultRenewed = loan.Renew(
         utcNow: utcNow,
         newDueDate: newDueDate
      );

      if (resultRenewed.IsFailure)
         return Result<LoanDto>.Failure(resultRenewed.Error);

      // Save all changes to database.
      var rows = await unitOfWork.SaveAllChangesAsync(
         "LoanUcRenew",
         ct
      );

      logger.LogDebug(
         "LoanUcRenew {LoanId} done, rows {Rows}",
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