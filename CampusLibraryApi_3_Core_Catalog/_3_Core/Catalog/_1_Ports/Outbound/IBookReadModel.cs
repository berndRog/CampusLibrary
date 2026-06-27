using CampusLibraryApi._2_BuildingBlocks;
using CampusLibraryApi._3_Core.Catalog._2_Application.Dtos;

namespace CampusLibraryApi._3_Core.Catalog._1_Ports.Outbound;

// Read model port for catalog book queries.
//
// This interface belongs to the query side of the Catalog module.
// It returns DTOs optimized for reading, searching and displaying books.
// It does not expose Book aggregates to controllers or clients.
public interface IBookReadModel {

   // Finds one book by its technical id.
   //
   // By default, only active books are returned.
   // If includeInactive is true, inactive books are included as well.
   //
   // Returns a detail DTO including book metadata, author text,
   // book items, item counts and lifecycle information.
   //
   // BookItemStatus is not an active/inactive flag. It represents the
   // fachlicher Zustand of a physical copy, for example Available, Lost
   // or Damaged.
   //
   // If the id is empty or no matching book exists for the given id,
   // a failure result is returned.
   Task<Result<BookDetailDto>> FindByIdAsync(
      Guid id,
      bool includeInactive = false,
      CancellationToken ct = default
   );

   // Selects books.
   //
   // By default, only active books are returned.
   // If includeInactive is true, inactive books are included as well.
   //
   // Returns list item DTOs optimized for overview screens.
   // The result includes title, subtitle, ISBN, author text and
   // the number of total and available book items.
   //
   // Available book items are determined by BookItemStatus.Available.
   // Other item statuses such as Unavailable, Lost or Damaged are still
   // part of the book's inventory, but they are not counted as available.
   Task<Result<IReadOnlyList<BookListItemDto>>> SelectAllAsync(
      bool includeInactive = false,
      CancellationToken ct = default
   );

   // Searches books by one selected search field.
   //
   // By default, only active books are searched.
   // If includeInactive is true, inactive books are included in the search.
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
      bool includeInactive = false,
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

Die normale Sicht auf den Catalog liefert nur aktive Books:

   includeInactive = false

Eine administrative oder interne Sicht kann inaktive Books einbeziehen:

   includeInactive = true

Damit wird dasselbe Prinzip wie im Readers-Modul verwendet. Es gibt keine
zusätzlichen Methoden wie FindByIdWithInactiveAsync oder
SelectAllWithInactiveAsync. Die Ressource bleibt dieselbe; nur die Sicht auf
die Ressource wird über einen Parameter erweitert.

Wichtig ist die fachliche Abgrenzung:

Book.IsActive
- steuert, ob ein Buch in normalen Catalog-Abfragen sichtbar ist
- wird über includeInactive beeinflusst

BookItem.Status
- beschreibt den Zustand eines physischen Exemplars
- zum Beispiel Available, Unavailable, Lost oder Damaged
- ist kein Ersatz für Book.IsActive
- wird für Detailinformationen und Zählwerte verwendet

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