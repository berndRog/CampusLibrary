using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using CampusLibraryApi._2_BuildingBlocks;
using CampusLibraryApi._3_Core.Loans._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Loans._2_Application.Dtos;
using CampusLibraryApi._3_Core.Loans._3_Domain.Entities;
using CampusLibraryApi._3_Core.Loans._3_Domain.Enums;
using CampusLibraryApi._3_Core.Loans._3_Domain.Errors;
using Microsoft.EntityFrameworkCore;

[assembly: InternalsVisibleTo("CampusLibraryApiTest")]
namespace CampusLibraryApi._4_Infrastructure._2_Persistence.ReadModels;

// EF Core implementation of the Loans read model.
// Read models return DTOs and do not expose domain aggregates.
internal sealed class LoanReadModelEf(
   ILoanDbContext loanDbContext
) : ILoanReadModel {

   // Finds one loan by its id.
   public async Task<Result<LoanDto>> FindByIdAsync(
      Guid id,
      CancellationToken ct
   ) {
      if (id == Guid.Empty)
         return Result<LoanDto>.Failure(LoanErrors.LoanIdRequired);

      LoanDto? dto = await loanDbContext.Loans
         .AsNoTracking()
         .Where(loan => loan.Id == id)
         .Select(ToLoanDtoExpr)
         .FirstOrDefaultAsync(ct);

      if (dto is null)
         return Result<LoanDto>.Failure(LoanErrors.LoanNotFound);
      return Result<LoanDto>.Success(dto);
   }

   // Returns all currently active loans.
   public async Task<Result<IReadOnlyList<LoanDto>>> FindAllActiveAsync(
      CancellationToken ct
   ) {
      IReadOnlyList<LoanDto> dtos = await loanDbContext.Loans
         .AsNoTracking()
         .Where(loan => loan.Status == LoanStatus.Active)
         .OrderBy(loan => loan.LoanPeriodVo.DueDate)
         .Select(ToLoanDtoExpr)
         .ToListAsync(ct);

      return Result<IReadOnlyList<LoanDto>>.Success(dtos);
   }

   // Returns all currently active loans for one reader.
   public async Task<Result<IReadOnlyList<LoanDto>>> FindActiveByReaderIdAsync(
      Guid readerId,
      CancellationToken ct
   ) {
      if (readerId == Guid.Empty)
         return Result<IReadOnlyList<LoanDto>>.Failure(
            LoanErrors.ReaderIdRequired
         );

      IReadOnlyList<LoanDto> dtos = await loanDbContext.Loans
         .AsNoTracking()
         .Where(loan => loan.ReaderId == readerId && 
                loan.Status == LoanStatus.Active)
         .OrderBy(loan => loan.LoanPeriodVo.DueDate)
         .Select(ToLoanDtoExpr)
         .ToListAsync(ct);

      return Result<IReadOnlyList<LoanDto>>.Success(dtos);
   }

   // Returns all active loans whose due date is before the given timestamp.
   public async Task<Result<IReadOnlyList<LoanDto>>> FindAllOverdueAsync(
      DateTime utcNow,
      CancellationToken ct
   ) {
      if (!IsValidUtc(value: utcNow))
         return Result<IReadOnlyList<LoanDto>>.Failure(LoanErrors.InvalidUtcNow);

      IReadOnlyList<LoanDto> dtos = await loanDbContext.Loans
         .AsNoTracking()
         .Where(loan => loan.Status == LoanStatus.Active &&
                loan.LoanPeriodVo.DueDate < utcNow)
         .OrderBy(loan => loan.LoanPeriodVo.DueDate)
         .Select(ToLoanDtoExpr)
         .ToListAsync(ct);

      return Result<IReadOnlyList<LoanDto>>.Success(dtos);
   }

   private static readonly Expression<Func<Loan, LoanDto>> ToLoanDtoExpr 
      = loan => new(
         loan.Id,
         loan.LoanPeriodVo.LoanDate,
         loan.LoanPeriodVo.DueDate,
         loan.ReaderId,
         loan.BookItemId,
         loan.ReturnedAt,
         loan.Status,
         loan.RenewalCount
      );
   
   // Checks whether a DateTime value is a valid UTC timestamp.
   private static bool IsValidUtc(
      DateTime value
   ) => value != default &&
        value.Kind == DateTimeKind.Utc;
}

/*
Lernziele und Didaktik
----------------------

Diese Klasse ist die EF-Core-Implementierung des Loan-ReadModels.

Das Interface ILoanReadModel liegt im Loans-Core. Die technische Umsetzung
liegt in Infrastructure. Dadurch bleibt der Core unabhängig von EF Core.

Ein ReadModel dient der Leseseite des Moduls. Es liefert DTOs und keine
Domain-Aggregates zurück. Dadurch wird sichtbar, dass Lesen und Schreiben
unterschiedliche Aufgaben haben.

Für schreibende Use Cases wie Rückgabe oder Verlängerung wird das Repository
verwendet. Dort werden Loan-Aggregates geladen und von EF Core getrackt.

Dieses ReadModel verwendet dagegen AsNoTracking, weil die geladenen Daten
nicht verändert werden sollen. Das ist für reine Abfragen effizienter und
macht die Absicht des Codes deutlich.

Die Projektion auf LoanDto erfolgt direkt in der EF-Core-Abfrage. Dadurch
werden nur die benötigten Daten gelesen und nicht unnötig vollständige
Aggregate aufgebaut.

FindAllActiveAsync zeigt die aktuellen Ausleihen.
FindActiveByReaderIdAsync zeigt die aktuellen Ausleihen eines Readers.
FindAllOverdueAsync zeigt aktive Ausleihen, deren DueDate bereits in der
Vergangenheit liegt.

Damit wird die Query-Seite des Loans-Moduls didaktisch von der Command-Seite
getrennt.
*/