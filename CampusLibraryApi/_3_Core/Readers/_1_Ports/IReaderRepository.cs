using CampusLibraryApi._3_Core.Readers._3_Domain.Entities;
using CampusLibraryApi._3_Core.Readers._3_Domain.ValueObjects;

namespace CampusLibraryApi._3_Core.Readers._1_Ports;

// Repository port for the Reader aggregate.
// Used by write-side use cases to load and store aggregate roots.
// Returns domain objects because repositories belong to the command side.
public interface IReaderRepository {
   // Find reader aggregate by technical identifier.
   Task<Reader?> FindByIdAsync(
      Guid id,
      CancellationToken ct
   );

   // Find reader aggregate by technical identity subject.
   Task<Reader?> FindBySubjectAsync(
      string subject,
      CancellationToken ct
   );

   // Find reader aggregate by normalized email value object.
   Task<Reader?> FindByEmailAsync(
      EmailVo emailVo,
      CancellationToken ct
   );

   // Check whether a subject is already used.
   Task<bool> ExistsBySubjectAsync(
      string subject,
      CancellationToken ct
   );

   // Add a new reader aggregate to the current unit of work.
   void Add(Reader reader);
}

/*
Didaktik
--------

Dieses Interface beschreibt das Repository für das Reader-Aggregate.

Ein Repository gehört zur schreibenden Seite der Anwendung. Es arbeitet
mit Aggregates und nicht mit DTOs. Dadurch können Use Cases fachliche
Regeln über das Aggregate ausführen, bevor Änderungen gespeichert werden.

Das Repository ist ein Port des Core-Moduls. Die konkrete EF-Core-
Implementierung liegt in der Infrastructure und bleibt ein technisches Detail.

Abgrenzung zum ReadModel:

Repository
- Command-Seite
- lädt und speichert Aggregates
- wird von Use Cases verwendet

ReadModel
- Query-Seite
- projiziert Daten in DTOs
- wird von GET-Endpunkten verwendet

Lernziele
---------

- Repository als Port des Anwendungskerns verstehen
- Aggregate nur über die Write-Seite verändern
- Infrastructure-Implementierungen vom Core entkoppeln
- Unterschied zwischen Aggregate-Zugriff und DTO-Projektion erkennen
*/
