using System.Runtime.CompilerServices;
using CampusLibraryApi._2_BuildingBlocks._1_Ports;
using CampusLibraryApi._3_Core.Loans._1_Ports.Outbound;
using Microsoft.EntityFrameworkCore;

[assembly: InternalsVisibleTo("CampusLibraryApiTest")]
namespace CampusLibraryApi._4_Infrastructure.Persistence.Contracts;

// EF Core implementation of the Loans contract used by the Readers module.
// This class is allowed to access the Loans table because Loans owns it.
internal sealed class LoanReaderContractEf(
   ILoanDbContext loanDbContext
) : ILoanReaderContract {

   public async Task<bool> ExistsForReaderAsync(
      Guid readerId,
      CancellationToken ct
   ) {
      if(readerId == Guid.Empty)
         return false;

      // Only current loans are stored. Therefore every matching row means
      // that the Reader still has one borrowed BookItem.
      return await loanDbContext.Loans
         .AsNoTracking()
         .AnyAsync(
            loan => loan.ReaderId == readerId,
            ct
         );
   }
}

/*
Lernziele und Didaktik
----------------------

Dieser Adapter implementiert einen Contract, den das Loans-Modul für das
Readers-Modul bereitstellt.

Der Reader-UseCase kann prüfen, ob ein Reader noch aktuelle Ausleihen besitzt,
ohne direkten Zugriff auf die Loans-Tabelle oder das Loan-Aggregate zu erhalten.

Die Implementierung liegt in Infrastructure, weil sie EF Core verwendet. Der
Contract liegt in BuildingBlocks, weil beide Module ihn kennen müssen.

In diesem Projekt werden nur aktuelle Ausleihen gespeichert. Bei der Rückgabe
eines Exemplars wird der zugehörige Loan gelöscht. Deshalb genügt
AnyAsync(...): Eine passende Zeile bedeutet, dass der Reader noch nicht
deaktiviert werden darf.
*/
