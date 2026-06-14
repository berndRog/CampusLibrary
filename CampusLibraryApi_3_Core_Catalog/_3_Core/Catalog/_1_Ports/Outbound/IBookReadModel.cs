using CampusLibraryApi._2_BuildingBlocks;
using CampusLibraryApi._3_Core.Catalog._2_Application.Dtos;

namespace CampusLibraryApi._3_Core.Catalog._1_Ports.Outbound;

// Read model interface for querying book data.
// Used by the web layer to retrieve catalog information without exposing
// the domain model. Returns DTOs because this port belongs to the query side.
public interface IBookReadModel {

   // Find one book by technical identifier.
   Task<Result<BookDetailDto>> FindByIdAsync(
      Guid id,
      CancellationToken ct = default
   );

   // Return all active books as list items.
   Task<Result<IReadOnlyList<BookListItemDto>>> SelectAllAsync(
      CancellationToken ct = default
   );

   // Search active books by exactly one criterion,
   // for example title, author name or ISBN.
   Task<Result<IReadOnlyList<BookListItemDto>>> SearchAsync(
      BookSearchDto search,
      CancellationToken ct = default
   );

   // Select all active books assigned to one author.
   Task<Result<IReadOnlyList<BookListItemDto>>> SelectByAuthorIdAsync(
      Guid authorId,
      CancellationToken ct = default
   );
}

/*
Lernziele und Didaktik
----------------------

Dieses ReadModel bildet die Query-Seite für Bücher im Catalog-Modul.

Die Methoden liefern keine Book-Aggregates zurück, sondern BookListItemDto.
Dieses DTO ist für die Anzeige optimiert und enthält zum Beispiel Titel,
Untertitel, ISBN, Autoren sowie die Anzahl der Exemplare.

Die Suche ist bewusst als ReadModel modelliert und nicht als Domain-Methode.
Die Domain entscheidet, welche fachlichen Regeln innerhalb eines Aggregates
gelten. Das ReadModel entscheidet, wie Daten für Listen, Suchen und Anzeigen
zusammengestellt werden.

Alle Methoden liefern Result<T>. Dadurch bleibt die Fehlerbehandlung im
Application-Layer einheitlich mit den vorhandenen Reader-ReadModels und den
UseCases.

Die Sortierung der Autoren eines Buches gehört ebenfalls auf die Query-Seite:
zuerst nach Nachname, danach nach Vorname.
*/