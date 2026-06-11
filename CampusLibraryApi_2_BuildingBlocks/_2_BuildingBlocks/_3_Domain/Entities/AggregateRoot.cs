using CampusLibraryApi._2_BuildingBlocks._3_Domain.Errors;
namespace CampusLibraryApi._2_BuildingBlocks._3_Domain.Entities;

// Base class for all aggregate roots in the domain model.
// Responsibilities:
// - Inherits identity semantics from Entity.
// - Manages audit timestamps (CreatedAt, UpdatedAt).
// - Receives UTC timestamps from the application layer.
// - Does not access the system clock directly.
public abstract class AggregateRoot : Entity {
   // Timestamp when the aggregate was created. Should only be set once.
   public DateTime CreatedAt { get; protected set; }

   // Timestamp of the last modification of the aggregate.
   public DateTime UpdatedAt { get; protected set; }

   protected AggregateRoot() {
      // Required by EF Core.
   }

   // Explicitly sets the creation timestamp.
   // Expected validation errors are returned as Result, not thrown.
   protected Result Initialize(DateTime createdAt) {
      if (createdAt == default)
         return Result.Failure(AggregateErrors.CreatedAtRequired);

      if (createdAt.Kind != DateTimeKind.Utc)
         return Result.Failure(AggregateErrors.CreatedAtMustBeUtc);

      CreatedAt = createdAt;
      UpdatedAt = createdAt;

      return Result.Success();
   }

   // Updates the modification timestamp.
   // Expected validation errors are returned as Result, not thrown.
   protected Result Touch(DateTime updatedAt) {
      if (updatedAt == default)
         return Result.Failure(AggregateErrors.UpdatedAtRequired);

      if (updatedAt.Kind != DateTimeKind.Utc)
         return Result.Failure(AggregateErrors.UpdatedAtMustBeUtc);

      if (CreatedAt != default && updatedAt < CreatedAt)
         return Result.Failure(AggregateErrors.UpdatedAtBeforeCreatedAt);

      UpdatedAt = updatedAt;

      return Result.Success();
   }
}

/*
Didaktik
--------

AggregateRoot erweitert Entity um den Lebenszyklus eines Aggregats.

Eine Entity beschreibt Identität. Ein Aggregate Root ist zusätzlich der
Einstiegspunkt in einen fachlichen Konsistenzbereich. Von außen sollen
Änderungen nur über das Aggregate Root erfolgen.

CreatedAt und UpdatedAt gehören zum Lebenszyklus des Aggregats. Der
Zeitpunkt wird aber nicht im Aggregate selbst erzeugt. Stattdessen wird
er vom Use Case übergeben, typischerweise aus einem IClock-Port.

Dadurch gilt:

- keine direkte Abhängigkeit auf DateTime.UtcNow in der Domain
- deterministische Tests durch kontrollierte Zeitwerte
- klare UTC-Regel für intern gespeicherte Zeitpunkte

CampusLibrary verwendet intern UTC-DateTime. DateTimeOffset bleibt eine
mögliche API-Grenzentscheidung, wird aber nicht für SQLite-Queries im
Persistenzmodell verwendet.

Initialize(...) und Touch(...) liefern Result zurück. Damit bleibt die
Fehlerstrategie konsistent: erwartbare Validierungsfehler werden nicht
als Exceptions geworfen, sondern als DomainError über Result transportiert.

Lernziele
---------

- Entity und AggregateRoot unterscheiden
- Aggregate Root als Konsistenz- und Transaktionsgrenze verstehen
- Audit-Felder als Teil des Aggregate-Lebenszyklus einordnen
- Zeit als Abhängigkeit über den Use Case zuführen
- UTC-DateTime als interne Zeitregel anwenden
- Result statt Exceptions für erwartbare Regelverletzungen nutzen
*/
