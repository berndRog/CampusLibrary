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

   // Finds the current loan for one concrete book item.
   // Because only current loans are stored, no status filter is required.
   Task<Loan?> FindBorrowedByBookItemIdAsync(
      Guid bookItemId,
      CancellationToken ct
   );

   // Finds all current loans for one reader.
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

   // Removes a loan after the book item was returned at the service desk.
   void Remove(
      Loan loan
   );
}

/*
Lernziele und Didaktik
----------------------

Dieses Repository ist ein Outbound-Port des Loans-Moduls.

Ein Repository arbeitet mit Aggregates. Es wird von schreibenden Use Cases
verwendet, wenn ein Loan geladen, geprüft, gespeichert oder gelöscht werden
muss.

Nur aktuell bestehende Ausleihen werden gespeichert. Daher bedeutet ein
gefundener Loan immer, dass das BookItem ausgeliehen ist. Bei der Rückgabe
wird der Loan über Remove aus der Persistenz entfernt.
*/