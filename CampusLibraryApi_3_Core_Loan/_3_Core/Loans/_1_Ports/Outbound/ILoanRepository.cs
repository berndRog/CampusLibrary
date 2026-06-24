using CampusLibraryApi._3_Core.Loans._3_Domain.Entities;

namespace CampusLibraryApi._3_Core.Loans._1_Ports.Outbound;

// Outbound port for loading and storing Loan aggregates.
// Command use cases use this repository to work with the Loan domain model.
public interface ILoanRepository {

   // Finds one loan aggregate by its id.
   Task<Loan?> FindByIdAsync(
      Guid id,
      CancellationToken ct
   );

   // Finds the currently active loan for one concrete book item.
   // This is needed to prevent lending the same physical copy twice.
   Task<Loan?> FindActiveByBookItemIdAsync(
      Guid bookItemId,
      CancellationToken ct
   );

   // Finds all active loans for one reader.
   // This can be used for domain checks such as maximum active loans.
   Task<IReadOnlyList<Loan>> FindActiveByReaderIdAsync(
      Guid readerId,
      CancellationToken ct
   );

   // Adds a new loan aggregate
   void Add(Loan loan);
   
   // Add a collection of loan aggregates
   void AddRange(IEnumerable<Loan> loans);

}

/*
Lernziele und Didaktik
----------------------

Dieses Repository ist ein Outbound-Port des Loans-Moduls.

Ein Repository arbeitet mit Aggregates. Es wird von schreibenden Use Cases
verwendet, wenn ein Loan geladen, geprüft oder neu gespeichert werden muss.

Das Repository gibt Loan-Aggregates zurück, keine DTOs. Dadurch bleibt der
Unterschied zwischen Domänenmodell und API-/ReadModel-Daten sichtbar.

Besonders wichtig ist FindActiveByBookItemIdAsync: Ein BookItem beschreibt
ein konkretes physisches Exemplar. Dieses Exemplar darf nicht gleichzeitig
mehrfach aktiv ausgeliehen sein. Diese fachliche Regel wird später im
Borrow-Use-Case geprüft.
*/