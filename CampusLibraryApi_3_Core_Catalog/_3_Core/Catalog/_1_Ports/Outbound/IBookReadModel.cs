using CampusLibraryApi._2_BuildingBlocks;
using CampusLibraryApi._3_Core.Catalog._2_Application.Dtos;

namespace CampusLibraryApi._3_Core.Catalog._1_Ports.Outbound;

// Read model port for catalog book queries.
// 
// This interface belongs to the query side of the Catalog module.
// It returns DTOs optimized for reading, searching and displaying books.
// It does not expose Book aggregates to controllers or clients.
public interface IBookReadModel {

   // Finds one active book by its technical id.
   //
   // Returns a detail DTO including book metadata, author text,
   // book items, item counts and lifecycle information.
   //
   // If the id is empty or no active book exists for the given id,
   // a failure result is returned.
   Task<Result<BookDetailDto>> FindByIdAsync(
      Guid id,
      CancellationToken ct = default
   );

   // Selects all active books.
   //
   // Returns list item DTOs optimized for overview screens.
   // The result includes title, subtitle, ISBN, author text and
   // the number of total and available book items.
   Task<Result<IReadOnlyList<BookListItemDto>>> SelectAllAsync(
      CancellationToken ct = default
   );

   // Searches active books by one selected search field.
   //
   // Supported search fields are defined by BookSearchDto / BookSearchField,
   // for example:
   //
   // - Title
   // - AuthorLastName
   // - Isbn
   //
   // AuthorLastName does not search an Author entity. In the simplified
   // Catalog model, authors are stored as a comma-separated text on Book.
   // The concrete read model implementation interprets this text for
   // author searches.
   //
   // If the search text is empty, an empty list is returned.
   Task<Result<IReadOnlyList<BookListItemDto>>> SearchAsync(
      BookSearchDto search,
      CancellationToken ct = default
   );
}

/*
Lernziele und Didaktik
----------------------

Diese Schnittstelle ist der lesende Port für Book-Abfragen im Catalog-Modul.

Sie gehört zur Query-Seite der Anwendung. Controller arbeiten nicht direkt mit
EF Core und erhalten auch keine Book-Aggregates. Stattdessen fragen sie über
dieses Interface DTOs ab, die für Anzeige und Suche vorbereitet sind.

Die Schnittstelle enthält bewusst nur lesende Operationen:

- FindByIdAsync für Detailansichten
- SelectAllAsync für Listenansichten
- SearchAsync für Suchanfragen

Schreibende Operationen wie Buch anlegen, Buch ändern, Buch deaktivieren oder
Exemplar hinzufügen gehören nicht in das ReadModel, sondern in Use Cases.

In dieser reduzierten Catalog-Version gibt es keine eigene Author-Entity mehr.
Autorinnen und Autoren werden als kommaseparierter Text im Book gespeichert.
Deshalb gibt es auch keine Methode SelectByAuthorIdAsync mehr. Eine Suche nach
Autor erfolgt stattdessen über SearchAsync mit dem Suchfeld AuthorLastName.

Didaktisch bleibt dadurch die Trennung sichtbar:

- Domain: schützt fachliche Regeln innerhalb des Aggregates.
- UseCase: verändert Zustand.
- Repository: lädt Aggregate für Änderungen.
- ReadModel: liefert Daten für Anzeige und Suche.
- Controller: verwendet Schnittstellen und kennt keine konkrete Persistenz.

Die komplexere fachliche Beziehung wird später im Loans-Modul behandelt.
Dort entsteht mit Loan ein eigener fachlicher Vorgang zwischen Reader und
BookItem. Im Catalog-Modul bleiben Autoren dagegen bewusst einfache Textdaten,
damit der Stoffumfang reduziert wird.
*/