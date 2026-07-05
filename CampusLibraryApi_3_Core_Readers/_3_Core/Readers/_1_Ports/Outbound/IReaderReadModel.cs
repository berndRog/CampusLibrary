using CampusLibraryApi._2_BuildingBlocks;
using CampusLibraryApi._3_Core.Readers._2_Application.Dtos;

namespace CampusLibraryApi._3_Core.Readers._1_Ports.Outbound;

// Read model interface for querying reader data.
// Used by the web layer to retrieve reader information without exposing
// the domain model. Returns DTOs because this port belongs to the query side.
public interface IReaderReadModel {

   // Returns the currently authenticated reader
   Task<Result<ReaderDto>> FindMeAsync(
      CancellationToken ct = default
   );
   
   // Finds a reader by technical identifier.
   // By default, only active readers are returned.
   // If includeInactive is true, inactive readers are included as well.
   Task<Result<ReaderDto>> FindByIdAsync(
      Guid id,
      bool includeInactive = false,
      CancellationToken ct = default
   );

   // Finds a reader by technical identity subject.
   // By default, only active readers are returned.
   // If includeInactive is true, inactive readers are included as well.
   Task<Result<ReaderDto>> FindBySubjectAsync(
      string subject,
      bool includeInactive = false,
      CancellationToken ct = default
   );

   // Finds a reader by normalized email address.
   // By default, only active readers are returned.
   // If includeInactive is true, inactive readers are included as well.
   Task<Result<ReaderDto>> FindByEmailAsync(
      string email,
      bool includeInactive = false,
      CancellationToken ct = default
   );

   // Returns readers as DTOs.
   // By default, only active readers are returned.
   // If includeInactive is true, inactive readers are included as well.
   Task<Result<IReadOnlyList<ReaderDto>>> SelectAllAsync(
      bool includeInactive = false,
      CancellationToken ct = default
   );
}

/*
Didaktik
--------

Dieses Interface beschreibt das ReadModel des Readers-Moduls.

Ein ReadModel wird für Abfragen verwendet und liefert DTOs
anstatt Domain-Objekten zurück. Dadurch kann die Lese-Seite
unabhängig vom Aggregate optimiert werden.

Die Sichtbarkeit aktiver und inaktiver Reader wird nicht mehr über
zusätzliche Methoden wie FindByIdWithInactiveAsync oder
SelectAllWithInactiveAsync modelliert. Stattdessen steuert der Parameter
includeInactive die Abfrage.

Standardfall:

   includeInactive = false

In diesem Fall liefert das ReadModel nur aktive Reader.

Erweiterte administrative Sicht:

   includeInactive = true

In diesem Fall werden aktive und inaktive Reader berücksichtigt.

Wichtiger Unterschied zum Repository:

Repository
- arbeitet mit Aggregates
- wird für schreibende Use Cases verwendet
- schützt fachliche Konsistenzregeln

ReadModel
- arbeitet mit DTOs
- wird für GET-Endpunkte, Listen und Suchabfragen verwendet
- darf direkt aus der Datenbank in DTOs projizieren
- darf Sichtbarkeitsfilter für die Query-Seite anbieten

Lernziele
---------

- Unterschied zwischen Repository und ReadModel verstehen
- GET-Endpunkte der Query-Seite zuordnen
- DTO-Projektionen als Alternative zum Laden von Aggregates erkennen
- Ports zur Entkopplung der Infrastructure einsetzen
- Standardsicht und administrative Sicht über Query-Parameter modellieren
*/