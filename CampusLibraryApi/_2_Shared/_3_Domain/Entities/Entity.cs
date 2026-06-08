namespace CampusLibraryApi._2_Shared._3_Domain.Entities;

// Base class for all domain entities.
// Identity semantics:
// - Equality is based solely on Id.
// - Two persisted entities are equal if their Id values are equal.
// - Transient entities (Id == Guid.Empty) are never considered equal.
public abstract class Entity : IEquatable<Entity> {
   // Technical primary key of the entity.
   // Must be unique and stable after creation.
   public Guid Id { get; protected set; }

   // Overrides object equality using DDD identity semantics.
   public override bool Equals(object? obj) {
      if (obj is not Entity other)
         return false;

      if (ReferenceEquals(this, other))
         return true;

      // Transient entities are never equal.
      if (Id == Guid.Empty || other.Id == Guid.Empty)
         return false;

      return Id == other.Id;
   }

   // Strongly typed equality implementation.
   public bool Equals(Entity? other) =>
      Equals((object?)other);

   // Hash code is derived from Id.
   // Do not use transient entities as stable keys in hash-based collections.
   public override int GetHashCode() =>
      Id.GetHashCode();

   // Equality operator overload.
   public static bool operator ==(Entity? a, Entity? b) {
      if (a is null && b is null) return true;
      if (a is null || b is null) return false;
      return a.Equals(b);
   }

   // Inequality operator overload.
   public static bool operator !=(Entity? a, Entity? b) =>
      !(a == b);
}

/*
Didaktik
--------

Entity ist die Basisklasse für Objekte mit fachlicher Identität.

Im Domain-Driven Design wird eine Entity nicht über alle Eigenschaften
verglichen, sondern über ihre Identität. Ein Reader bleibt also derselbe
Reader, auch wenn sich Name, Adresse oder E-Mail-Adresse ändern.

Die Id ist hier ein Guid und damit eine technische Identität. Die fachliche
Bedeutung entsteht erst in den konkreten Aggregates, z. B. Reader oder Book.

Wichtig ist die Behandlung transienter Entities:

- Guid.Empty bedeutet: noch keine gültige Identität gesetzt
- solche Objekte gelten niemals als gleich
- dadurch werden Fehler in Collections und beim EF-Core-Tracking reduziert

String-Parsing oder das Erzeugen einer neuen Guid aus optionalen API-Daten
gehört nicht in Entity. Diese Logik liegt bewusst außerhalb der Domain-Basis,
z. B. in EntityId.Resolve(...) in der Application-/Utility-Schicht.

Lernziele
---------

- Unterschied zwischen Referenzgleichheit und Identitätsgleichheit verstehen
- Entity und Value Object voneinander abgrenzen
- Datenbankidentität und fachlichen Objektzustand unterscheiden
- transiente Entities korrekt behandeln
- Guid als einfache technische Identität einordnen
- erkennen, warum Parsing-Logik nicht in die Entity-Basisklasse gehört
*/
