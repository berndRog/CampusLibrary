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
   ILogger<LoanUcReturnAtDesk> logger
) {

   public async Task<Result<LoanDto>> ExecuteAsync(
      Guid loanId,
      CancellationToken ct = default
   ) {
      if(loanId == Guid.Empty)
         return Result<LoanDto>.Failure(LoanErrors.LoanIdRequired);

      // Only current loans are stored. Finding the loan therefore means that
      // the referenced book item is currently borrowed.
      var loan = await loanRepository.FindByIdAsync(
         id: loanId,
         ct: ct
      );

      if(loan is null)
         return Result<LoanDto>.Failure(LoanErrors.LoanNotFound);

      // Keep the response data before deleting the aggregate.
      LoanDto returnedLoanDto = loan.ToLoanDto();

      // Returning at the desk ends the loan lifecycle. No returned loan is
      // retained in this simplified model.
      loanRepository.Remove(
         loan: loan
      );

      var rows = await unitOfWork.SaveAllChangesAsync(
         "LoanUcReturnAtDesk",
         ct
      );

      logger.LogDebug(
         "LoanUcReturnAtDesk deleted loan {LoanId}, rows {Rows}",
         loan.Id,
         rows
      );

      return Result<LoanDto>.Success(returnedLoanDto);
   }
}

/*
Lernziele und Didaktik
----------------------

Dieser Use Case beschreibt die Rückgabe eines ausgeliehenen Exemplars am
Service Desk.

Ein Loan repräsentiert ausschließlich eine aktuelle Ausleihe. Deshalb wird
bei der Rückgabe kein Status gesetzt und kein ReturnedAt gespeichert. Der
Use Case löscht stattdessen den Loan über das Repository.

Die Rückgabehistorie wird in dieser didaktisch vereinfachten Version bewusst
nicht gespeichert. Soll später eine Historie benötigt werden, wäre dafür ein
eigenes Archiv- oder History-Konzept geeigneter als ein zweiter Zustand im
aktuellen Loan-Aggregate.
*/
