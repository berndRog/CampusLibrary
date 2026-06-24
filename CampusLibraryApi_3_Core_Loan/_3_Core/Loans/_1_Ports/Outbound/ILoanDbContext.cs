using CampusLibraryApi._3_Core.Loans._3_Domain.Entities;

namespace CampusLibraryApi._3_Core.Loans._1_Ports.Outbound;

// DbContext port for the Loans module.
// Exposes only persistence access needed by this module.
public interface ILoanDbContext {

   // Query access to Loan aggregates.
   IQueryable<Loan> Loans { get; }

   // Adds one loan aggregate to the persistence context.
   void Add(Loan loan);

   // Adds multiple loan aggregates to the persistence context.
   void AddRange(IEnumerable<Loan> loans);
}

/*
Lernziele und Didaktik
----------------------

Dieses Interface begrenzt den Datenbankzugriff des Loans-Moduls.

Das Loans-Modul erhält nur Zugriff auf Loans. Es bekommt hier keinen direkten
Zugriff auf Readers, Books oder BookItems.

Dadurch bleibt die Modulgrenze sichtbar: Das Loans-Modul besitzt die
Loan-Tabelle, aber nicht die Reader- oder Catalog-Tabellen.

Wenn Loans Informationen über Reader oder BookItems benötigt, geschieht das
nicht über diesen DbContext-Port, sondern über Contracts der besitzenden
Module.
*/