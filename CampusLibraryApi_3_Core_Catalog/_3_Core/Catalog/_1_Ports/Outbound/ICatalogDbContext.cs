using CampusLibraryApi._3_Core.Catalog._3_Domain.Entities;
namespace CampusLibraryApi._3_Core.Catalog._1_Ports.Outbound;

// DbContext port for the Catalog module.
// Exposes only the persistence operations needed by this module.
// The concrete EF Core implementation is provided by Infrastructure.
public interface ICatalogDbContext {
   // Query access to the Book aggregates.
   IQueryable<Book> Books { get; }
   IQueryable<BookItem> BookItems { get; }

   // Add a new book to the persistence context.
   void Add(Book book);

   // Add multiple books to the persistence context.
   void AddRange(IEnumerable<Book> books);
   
}

/*
Didaktik
--------

Dieses Interface begrenzt den Zugriff des Readers-Moduls auf die
Datenbank. Obwohl technisch ein gemeinsamer DbContext existiert,
sieht das Modul nur die Tabellen und Operationen, die es benötigt.

Das ist eine pragmatische Lösung für einen modularen Monolithen:
Die Datenbank bleibt gemeinsam, die fachlichen Module behalten aber
eine klarere Grenze.

Lernziele
---------

- Gemeinsamen DbContext und modulbezogene Sicht unterscheiden
- Infrastructure hinter einem Port verstecken
- Zugriff auf fremde Tabellen fachlich begrenzen
- Modularisierung innerhalb eines API-Projekts nachvollziehen
*/
