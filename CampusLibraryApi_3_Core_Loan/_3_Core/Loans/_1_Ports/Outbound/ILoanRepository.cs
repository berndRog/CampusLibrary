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

   // Finds the currently borrowed loan for one concrete book item.
   // This is needed to prevent lending the same physical copy twice.
   Task<Loan?> FindBorrowedByBookItemIdAsync(
      Guid bookItemId,
      CancellationToken ct
   );

   // Finds all currently borrowed loans for one reader.
   // This can be used for domain checks such as maximum borrowed loans.
   Task<IReadOnlyList<Loan>> FindBorrowedByReaderIdAsync(
      Guid readerId,
      CancellationToken ct
   );

   // Adds a new loan aggregate.
   void Add(
      Loan loan
   );

   // Adds a collection of loan aggregates.
   void AddRange(
      IEnumerable<Loan> loans
   );
}

/*
Lernziele und Didaktik
----------------------

Dieses Repository ist ein Outbound-Port des Loans-Moduls.

Ein Repository arbeitet mit Aggregates. Es wird von schreibenden Use Cases
verwendet, wenn ein Loan geladen, geprüft oder neu gespeichert werden muss.

Das Repository gibt Loan-Aggregates zurück, keine DTOs. Dadurch bleibt der
Unterschied zwischen Domänenmodell und API-/ReadModel-Daten sichtbar.

Loans besitzen kein IsActive-Flag. Der fachliche Zustand einer Ausleihe wird
über LoanStatus modelliert.

Deshalb heißen die Suchmethoden nicht FindActive..., sondern FindBorrowed...:

- FindBorrowedByBookItemIdAsync sucht die offene Ausleihe eines konkreten
  physischen Exemplars.
- FindBorrowedByReaderIdAsync sucht alle offenen Ausleihen eines Readers.

Ein BookItem beschreibt ein konkretes physisches Exemplar. Dieses Exemplar
darf nicht gleichzeitig mehrfach ausgeliehen sein. Diese fachliche Regel wird
im Borrow-Use-Case geprüft.
*/