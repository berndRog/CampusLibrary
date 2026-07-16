using CampusLibraryApi._3_Core.Catalog._2_Application.Dtos;
using CampusLibraryApi._3_Core.Catalog._2_Application.Enums;
using CampusLibraryApi._2_BuildingBlocks;

namespace CampusLibraryApi._3_Core.Catalog._1_Ports.Outbound;

// Query-side port for Book resources. List, search and detail operations use
// the same public BookDto so the API contract remains small and predictable.
public interface IBookReadModel {

   Task<Result<BookDto>> FindByIdAsync(
      Guid id,
      bool includeInactive = false,
      CancellationToken ct = default
   );

   Task<Result<BookDeactivationInfoDto>> FindDeactivationInfoAsync(
      Guid id,
      CancellationToken ct = default
   );

   Task<Result<IReadOnlyList<BookDto>>> SelectAllAsync(
      bool includeInactive = false,
      CancellationToken ct = default
   );

   // The two HTTP query values are passed directly. A separate DTO would only
   // repeat the route/query parameters without adding a fachliche meaning.
   Task<Result<IReadOnlyList<BookDto>>> SearchAsync(
      BookSearchField searchField,
      string searchText,
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