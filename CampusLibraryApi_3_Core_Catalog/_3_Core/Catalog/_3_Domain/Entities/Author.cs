using CampusLibraryApi._2_BuildingBlocks;
using CampusLibraryApi._2_BuildingBlocks._3_Domain.Entities;
using CampusLibraryApi._3_Core.Catalog._3_Domain.Errors;

namespace CampusLibraryApi._3_Core.Catalog._3_Domain.Entities;

// Aggregate root for an author in the Catalog module.
// Represents a person who contributed to one or more books.
// Identity and timestamps are inherited from Entity / AggregateRoot.
public sealed class Author : AggregateRoot {

   //--- properties ------------------------------------------------------------
   // Inherited from Entity / AggregateRoot:
   // public Guid Id { get; protected set; }
   // public DateTime CreatedAt { get; protected set; }
   // public DateTime UpdatedAt { get; protected set; }
   public string Firstname { get; private set; } = string.Empty;
   public string Lastname { get; private set; } = string.Empty;
   public string DisplayName => $"{Firstname} {Lastname}".Trim();
   public bool IsActive { get; private set; } = true;
   
   //--- constructors ----------------------------------------------------------
   // Required by EF Core.
   private Author() {
   }
   // Domain ctor
   private Author(
      Guid id,
      string firstname,
      string lastname
   ) {
      Id = id;
      Firstname = firstname;
      Lastname = lastname;
   }
   
   //--- factory methods -------------------------------------------------------
   // Creates a new Author aggregate and initializes its UTC timestamps.
   // Validation errors are returned as Result failures.
   public static Result<Author> Create(
      Guid id,
      string firstname,
      string lastname,
      DateTime createdAt
   ) {
      firstname = firstname.Trim();
      lastname = lastname.Trim();
      
      // The id is resolved outside the domain entity, e.g. in a use case.
      if (id == Guid.Empty)
         return Result<Author>.Failure(CatalogErrors.AuthorIdRequired);

      if (string.IsNullOrWhiteSpace(firstname))
         return Result<Author>.Failure(CatalogErrors.FirstnameIsRequired);
      if (firstname.Length is < 2 or > 80)
         return Result<Author>.Failure(CatalogErrors.InvalidFirstname);
   
      if (string.IsNullOrWhiteSpace(lastname))
         return Result<Author>.Failure(CatalogErrors.LastnameIsRequired);
      if (lastname.Length is < 2 or > 80)
         return Result<Author>.Failure(CatalogErrors.InvalidLastname);
      
      // Create the aggregate with normalized string values.
      var author = new Author(
         id: id,
         firstname: firstname,
         lastname: lastname
      );

      // Initialize CreatedAt and UpdatedAt using the inherited lifecycle method.
      var resultInitialized = author.Initialize(createdAt);
      if (resultInitialized.IsFailure)
         return Result<Author>.Failure(resultInitialized.Error);

      return Result<Author>.Success(author);
   }

   //--- domain methods --------------------------------------------------------
   // Deactivate the author
   public Result Deactivate(
      DateTime updatedAt
   ) {
      if (!IsActive)
         return Result.Success();

      IsActive = false;
      
      var resultUpdated = Touch(updatedAt);
      if (resultUpdated.IsFailure)
         return Result.Failure(resultUpdated.Error);

      return Result.Success();
   }
}

/*
Lernziele und Didaktik
----------------------

Diese Klasse zeigt Author als eigenes Aggregate Root im Catalog-Modul.
Ein Author repräsentiert eine Autorin oder einen Autor, also eine fachliche
Person, die an einem oder mehreren Books beteiligt sein kann.

Author ist ein Aggregate Root, weil ein Author unabhängig von einem
einzelnen Book existieren kann. Dieselbe Person kann mehreren Büchern
zugeordnet werden. Würde Author nur als untergeordnete Entity von Book
modelliert, müsste dieselbe Person mehrfach in verschiedenen Books
gespeichert werden.

Die Beziehung zwischen Book und Author wird deshalb nicht direkt durch eine
Liste von Author-Objekten im Book modelliert, sondern über die Join Entity
BookAuthor. Dadurch entsteht eine m:n-Beziehung:

Book -> BookAuthor -> Author

Book ist dabei das Aggregate Root für die Buchseite der Beziehung.
Author ist ein eigenes Aggregate Root für die Personenseite der Beziehung.
BookAuthor beschreibt die Zuordnung zwischen beiden.

Zum Author-Aggregate gehören aktuell:

- Author als Aggregate Root
- Firstname und Lastname als einfache Eigenschaften
- DisplayName als abgeleiteter Anzeigename

Ein eigenes Value Object für den Namen wäre später möglich, ist für diesen
Teil des Projekts aber noch nicht zwingend erforderlich. Für den Einstieg
ist es didaktisch einfacher, Firstname und Lastname direkt im Author zu
halten.

Die Id wird nicht in Author aus einem string erzeugt. Das Auflösen optionaler
API- oder Testdaten geschieht außerhalb der Entity, zum Beispiel über
EntityId.Resolve(...) im Use Case oder im Seed. Author.Create(...) erhält
bereits einen Guid.

Die Zeitpunkte createdAt und updatedAt werden als DateTime übergeben und
müssen UTC sein. Das wird in AggregateRoot.Initialize(...) und
AggregateRoot.Touch(...) geprüft. Die Domain greift bewusst nicht direkt
auf DateTime.UtcNow zu.

Create(...) verwendet Initialize(createdAt). Dadurch werden CreatedAt und
UpdatedAt beim Erzeugen des Aggregates initial auf denselben UTC-Zeitpunkt
gesetzt.

Rename(...) ist eine fachliche Änderungsmethode. Sie verändert den Namen
des Authors und ruft Touch(updatedAt) auf. Dadurch wird sichtbar, wann das
Aggregate zuletzt geändert wurde.

Didaktisch zeigt diese Klasse mehrere zentrale DDD-Ideen:

- Author als eigenständiges Aggregate Root
- Aggregate Root als fachlicher Einstiegspunkt
- Trennung von Book und Author bei m:n-Beziehungen
- Join Entity BookAuthor als Verbindung zwischen Aggregates
- kontrollierte Zustandsänderung über fachliche Methoden
- UTC-Zeit als externe Abhängigkeit
- testbare Zeitsteuerung über FakeClock oder feste Zeitpunkte
- Result statt Exceptions für erwartbare Validierungsfehler
*/