using CampusLibraryApi._2_BuildingBlocks;
using CampusLibraryApi._3_Core.Catalog._2_Application.Dtos;

namespace CampusLibraryApi._3_Core.Catalog._1_Ports.Outbound;

// Read model interface for querying author data.
// Used by the web layer to retrieve author information without exposing
// the domain model. Returns DTOs because this port belongs to the query side.
public interface IAuthorReadModel {

   // Find author by technical identifier.
   Task<Result<AuthorDto>> FindByIdAsync(
      Guid id,
      CancellationToken ct = default
   );

   // Return all active authors as DTOs.
   Task<Result<IReadOnlyList<AuthorDto>>> SelectAllAsync(
      CancellationToken ct = default
   );

   // Search active authors by firstname, lastname or display name.
   Task<Result<IReadOnlyList<AuthorDto>>> SearchAsync(
      string searchText,
      CancellationToken ct = default
   );
}

/*
Lernziele und Didaktik
----------------------

Dieses ReadModel gehört zur Query-Seite des Catalog-Moduls.

Es liefert keine Domain-Objekte zurück, sondern AuthorDto. Dadurch bleibt die
Web-Schicht von der Domain-Schicht entkoppelt.

Alle Methoden liefern Result<T>. Damit können auch lesende Operationen
kontrolliert fachliche Fehler zurückgeben, zum Beispiel wenn eine Id ungültig
ist oder kein Autor gefunden wurde.

Wichtig ist die Unterscheidung zwischen Repository und ReadModel:

Das Repository lädt Aggregate für fachliche Änderungen.
Das ReadModel liefert Daten für Anzeige, Suche und Auswahl.

Inaktive Autoren werden nicht global per EF QueryFilter ausgeblendet, weil sie
in bestehenden Book-Author-Beziehungen weiterhin sichtbar bleiben sollen.
Für normale Autorenlisten und Suchfunktionen filtert das ReadModel daher
explizit auf IsActive == true.
*/