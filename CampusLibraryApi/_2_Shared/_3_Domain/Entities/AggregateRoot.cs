using CampusLibraryApi._2_Shared._3_Domain.Errors;

namespace CampusLibraryApi._2_Shared._3_Domain.Entities;

// Base class for all aggregate roots in the domain model.
//
// Responsibilities:
// - Inherits identity semantics from Entity.
// - Manages audit timestamps (CreatedAt, UpdatedAt).
// - Receives timestamps from the application layer.
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
   // Expected domain/application errors are returned as Result, not thrown.
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
   // Expected domain/application errors are returned as Result, not thrown.
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
================================================================================
DIDAKTIK UND LERNZIELE (Aggregate Root – Minimalversion)
================================================================================

1. Unterschied zwischen Entity und AggregateRoot verstehen
   - Entity: Identitätskonzept.
   - AggregateRoot: Einstiegspunkt eines Konsistenzbereichs (Consistency Boundary).

2. Zeit als Abhängigkeit modellieren
   - Kein direkter Zugriff auf DateTime.UtcNow im Aggregate.
   - Der Zeitpunkt wird von außen übergeben, typischerweise aus IClock im Use Case.
   - Ermöglicht deterministische Unit Tests.

3. Audit-Felder als Domänenverantwortung
   - CreatedAt und UpdatedAt gehören zum Lebenszyklus des Aggregats.
   - Touch(updatedAt) verdeutlicht explizite Zustandsänderung.

4. Konsistenzgrenzen begreifen
   - Nur AggregateRoot darf von außen referenziert werden.
   - Innere Entities sind nur über das Root manipulierbar.

5. Fehlerstrategie konsistent anwenden
   - Erwartbare Regelverletzungen werden als Result zurückgegeben.
   - Exceptions bleiben technischen oder programmatischen Fehlern vorbehalten.

Zentrales Lernziel:
Studierende sollen verstehen, dass ein Aggregate Root
nicht nur eine Entity mit zusätzlichen Feldern ist,
sondern eine fachliche Konsistenz- und Transaktionsgrenze
im Sinne von Domain-Driven Design darstellt.

================================================================================
*/
