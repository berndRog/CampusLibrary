using System.Runtime.CompilerServices;
using CampusLibraryApi._3_Core.Loans._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Loans._3_Domain.Entities;
using CampusLibraryApi._3_Core.Loans._3_Domain.Enums;
using Microsoft.EntityFrameworkCore;
[assembly: InternalsVisibleTo("CampusLibraryApiTest")]
namespace CampusLibraryApi._4_Infrastructure.Persistence.Loans;

// EF Core implementation of the Loan repository.
// The repository loads and stores Loan aggregates for command use cases.
internal sealed class LoanRepositoryEf(
   ILoanDbContext loanDbContext
) : ILoanRepository {

   // Finds one Loan aggregate by its id.
   // The returned aggregate is tracked by EF Core and can be changed by a use case.
   public async Task<Loan?> FindByIdAsync(
      Guid id,
      CancellationToken ct
   ) => await loanDbContext.Loans
      .FirstOrDefaultAsync(loan => loan.Id == id, ct);

   // Finds the currently active loan for one concrete book item.
   // This is used to prevent lending the same physical copy twice.
   public async Task<Loan?> FindActiveByBookItemIdAsync(
      Guid bookItemId,
      CancellationToken ct
   ) => await loanDbContext.Loans
      .FirstOrDefaultAsync(loan => loan.BookItemId == bookItemId &&
                           loan.Status == LoanStatus.Active, ct);

   // Finds all active loans for one reader.
   // This can be used for domain or application rules, for example
   // to limit the number of active loans per reader.
   public async Task<IReadOnlyList<Loan>> FindActiveByReaderIdAsync(
      Guid readerId,
      CancellationToken ct
   ) => await loanDbContext.Loans
      .Where(loan => loan.ReaderId == readerId &&
             loan.Status == LoanStatus.Active)
      .ToListAsync(ct);

   // Adds a new Loan aggregate to the persistence context.
   // The actual database write happens later through IUnitOfWork.
   public void Add(Loan loan) 
      => loanDbContext.Add(loan: loan);

   public void AddRange(IEnumerable<Loan> loans) 
      => loanDbContext.AddRange(loans);
}

/*
Lernziele und Didaktik
----------------------

Diese Klasse ist die EF-Core-Implementierung des Loan-Repositories.

Das Interface ILoanRepository liegt im Loans-Core. Die konkrete technische
Implementierung liegt in Infrastructure. Dadurch bleibt der Core unabhängig
von EF Core.

Ein Repository arbeitet mit Aggregates, nicht mit DTOs. Deshalb gibt dieses
Repository Loan-Objekte zurück. Die DTO-Erzeugung gehört nicht hierher,
sondern in Mappings, UseCases oder ReadModels.

Die Methode FindByIdAsync wird später für schreibende UseCases wie Rückgabe
und Verlängerung benötigt. Der geladene Loan bleibt durch EF Core getrackt,
damit Änderungen am Aggregate gespeichert werden können.

Die Methode FindActiveByBookItemIdAsync unterstützt eine wichtige Fachregel:
Ein konkretes physisches Exemplar darf nicht gleichzeitig mehrfach aktiv
ausgeliehen sein.

Die Methode FindActiveByReaderIdAsync kann für weitere Regeln verwendet
werden, zum Beispiel für eine maximale Anzahl aktiver Ausleihen pro Reader.

Add fügt den neuen Loan nur dem Persistence Context hinzu. Gespeichert wird
erst später über IUnitOfWork. Dadurch bleiben Repository und Transaktion
getrennt.
*/