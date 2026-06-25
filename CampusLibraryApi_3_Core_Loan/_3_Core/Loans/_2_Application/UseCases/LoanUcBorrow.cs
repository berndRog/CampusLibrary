using System.Runtime.CompilerServices;
using CampusLibraryApi._2_BuildingBlocks;
using CampusLibraryApi._2_BuildingBlocks._1_Ports;
using CampusLibraryApi._2_BuildingBlocks._1_Ports.Contracts;
using CampusLibraryApi._2_BuildingBlocks._2_Application.Utils;
using CampusLibraryApi._3_Core.Loans._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Loans._2_Application.Dtos;
using CampusLibraryApi._3_Core.Loans._2_Application.Mappings;
using CampusLibraryApi._3_Core.Loans._3_Domain.Entities;
using CampusLibraryApi._3_Core.Loans._3_Domain.Errors;
using CampusLibraryApi._3_Core.Loans._3_Domain.Policies;
using CampusLibraryApi._3_Core.Loans._3_Domain.ValueObjects;
using Microsoft.Extensions.Logging;

[assembly: InternalsVisibleTo("CampusLibraryApiTest")]
namespace CampusLibraryApi._3_Core.Loans._2_Application.UseCases;

internal sealed class LoanUcBorrow(
   ILoanRepository loanRepository,
   IReaderLoanContract readerLoanContract,
   IBookItemLoanContract bookItemLoanContract,
   IUnitOfWork unitOfWork,
   IClock clock,
   ILogger<LoanUcBorrow> logger
) {

   public async Task<Result<LoanDto>> ExecuteAsync(
      LoanCreateDto? loanCreateDto,
      CancellationToken ct = default
   ) {
      if (loanCreateDto is null)
         return Result<LoanDto>.Failure(LoanErrors.LoanCreateDtoRequired);
      var dto = loanCreateDto;

      // Resolve the optional external id into a domain id.
      var resultId = EntityId.Resolve(dto.Id, LoanErrors.InvalidLoanId);
      if (resultId.IsFailure)
         return Result<LoanDto>.Failure(resultId.Error);

      // Validate the referenced reader id.
      if (dto.ReaderId == Guid.Empty)
         return Result<LoanDto>.Failure(LoanErrors.ReaderIdRequired);

      // Validate the referenced book item id.
      if (dto.BookItemId == Guid.Empty)
         return Result<LoanDto>.Failure(LoanErrors.BookItemIdRequired);

      // Ask the Readers module whether this reader may borrow book items.
      // Loans does not access the Reader aggregate or the Readers table directly.
      var resultReader = await readerLoanContract
         .FindReaderForLoanAsync(dto.ReaderId, ct);
      if (resultReader.IsFailure)
         return Result<LoanDto>.Failure(resultReader.Error);

      // Ask the Catalog module whether this concrete book item is loanable.
      // Loans does not access Book or BookItem entities directly.
      var resultBookItem = await bookItemLoanContract
         .FindByIdAsync(dto.BookItemId, ct);
      if (resultBookItem.IsFailure)
         return Result<LoanDto>.Failure(resultBookItem.Error);
      var bookItem = resultBookItem.Value;

      if (!bookItem.IsAvailableForLoan)
         return Result<LoanDto>.Failure(LoanErrors.BookItemNotAvailable);

      // Check whether this concrete book item is already actively borrowed.
      var activeLoan = await loanRepository
         .FindActiveByBookItemIdAsync(dto.BookItemId, ct);
      if (activeLoan is not null)
         return Result<LoanDto>.Failure(LoanErrors.BookItemAlreadyBorrowed);

      // Create the loan period using the domain rules of the Loans module.
      var loanDate = clock.UtcNow;
      var dueDate = loanDate.AddDays(LoanRules.StandardLoanDays);

      var loanPeriodResult = LoanPeriodVo.Create(loanDate, dueDate);
      if (loanPeriodResult.IsFailure)
         return Result<LoanDto>.Failure(loanPeriodResult.Error);

      // Create the Loan aggregate.
      var resultLoan = Loan.Create(
         id: resultId.Value,
         readerId: dto.ReaderId,
         bookItemId: dto.BookItemId,
         loanPeriodVo: loanPeriodResult.Value
      );
      if (resultLoan.IsFailure)
         return Result<LoanDto>.Failure(resultLoan.Error);
      var loan = resultLoan.Value;

      // Add to repository.
      loanRepository.Add(loan);

      // Save all changes to database.
      var rows = await unitOfWork.SaveAllChangesAsync("LoanUcBorrow", ct);

      logger.LogDebug("LoanUcBorrow {LoanId} done, rows {Rows}",
         loan.Id, rows);

      return Result<LoanDto>.Success(loan.ToLoanDto());
   }
}

/*
Lernziele und Didaktik
----------------------

Dieser Use Case beschreibt den fachlichen Vorgang "Ausleihe anlegen".

Das Loans-Modul besitzt die Loan-Daten. Es besitzt aber weder Reader noch
BookItems. Deshalb greift dieser Use Case nicht direkt auf Reader- oder
Catalog-Tabellen zu.

Stattdessen werden die besitzenden Module über Contracts gefragt:

- Readers entscheidet, ob ein Reader existiert und aktiv ist.
- Catalog entscheidet, ob ein BookItem existiert und grundsätzlich ausleihbar ist.
- Loans entscheidet, ob dieses BookItem aktuell bereits aktiv ausgeliehen ist.

Dadurch bleibt die fachliche Zuständigkeit klar getrennt.

Der Client liefert keine Leihdauer. Die Leihdauer ist eine fachliche Regel
des Loans-Moduls und wird hier aus LoanRules und LoanPeriodVo gebildet.

Das Repository speichert Loan-Aggregates. Die Transaktion wird über
IUnitOfWork abgeschlossen. Damit bleiben Repository und Speichern getrennt.

Der Use Case gibt am Ende ein LoanDto zurück. Das Domain-Aggregate bleibt
innerhalb des Anwendungskerns geschützt.
*/