using CampusLibraryApi._2_BuildingBlocks;
using CampusLibraryApi._3_Core.Readers._2_Application.Dtos;

namespace CampusLibraryApi._3_Core.Readers._1_Ports;

// Read model interface for querying reader data.
// Used by the web layer to retrieve reader information without exposing
// the domain model. Returns DTOs because this port belongs to the query side.
public interface IReaderReadModel {
   // Find reader by technical identifier.
   Task<Result<ReaderDto>> FindByIdAsync(
      Guid id,
      CancellationToken ct = default
   );

   // Find reader by technical identity subject.
   Task<Result<ReaderDto>> FindBySubjectAsync(
      string subject,
      CancellationToken ct
   );

   // Find reader by normalized email address.
   Task<Result<ReaderDto>> FindByEmailAsync(
      string email,
      CancellationToken ct
   );

   // Return all readers as DTOs.
   Task<Result<IReadOnlyList<ReaderDto>>> SelectAllAsync(CancellationToken ct);
}

/*
Didaktik
--------

Dieses Interface beschreibt das ReadModel des Readers-Moduls.

Ein ReadModel wird für Abfragen verwendet und liefert DTOs
anstatt Domain-Objekten zurück. Dadurch kann die Lese-Seite
unabhängig vom Aggregate optimiert werden.

Wichtiger Unterschied zum Repository:

Repository
- arbeitet mit Aggregates
- wird für schreibende Use Cases verwendet
- schützt fachliche Konsistenzregeln

ReadModel
- arbeitet mit DTOs
- wird für GET-Endpunkte, Listen und Suchabfragen verwendet
- darf direkt aus der Datenbank in DTOs projizieren

Lernziele
---------

- Unterschied zwischen Repository und ReadModel verstehen
- GET-Endpunkte der Query-Seite zuordnen
- DTO-Projektionen als Alternative zum Laden von Aggregates erkennen
- Ports zur Entkopplung der Infrastructure einsetzen
*/
