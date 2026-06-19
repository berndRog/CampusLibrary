using CampusLibraryApi._2_BuildingBlocks;
using CampusLibraryApi._2_BuildingBlocks._2_Application.Utils;
using CampusLibraryApi._2_BuildingBlocks._3_Domain.Entities;
using CampusLibraryApi._3_Core.Catalog._3_Domain.Errors;
using CampusLibraryApi._3_Core.Catalog._3_Domain.Services;
using CampusLibraryApi._3_Core.Catalog._3_Domain.ValueObjects;

namespace CampusLibraryApi._3_Core.Catalog._3_Domain.Entities;

// Aggregate root for a book in the Catalog module.
// Represents the bibliographic work, not a physical copy.
// Identity and timestamps are inherited from AggregateRoot / Entity.
public sealed class Book : AggregateRoot {
   //--- properties ------------------------------------------------------------
   // Inherited from Entity / AggregateRoot:
   // public Guid Id { get; protected set; }
   // public DateTime CreatedAt { get; protected set; }
   // public DateTime UpdatedAt { get; protected set; }
   public string AuthorsText { get; private set; } = string.Empty;
   public string Title { get; private set; } = string.Empty;
   public string? Subtitle { get; private set; }
   public IsbnVo IsbnVo { get; private set; } = null!;
   public bool IsActive { get; private set; } = true;

   // Book -> BookItem [1] : [0,n]
   // A BookItem represents one physical copy of this book.
   private readonly List<BookItem> _bookItems = [];
   public IReadOnlyCollection<BookItem> BookItems => _bookItems.AsReadOnly();

   //--- constructors ----------------------------------------------------------
   // Required by EF Core.
   private Book() {
   }
   // Domain ctor
   private Book(
      Guid id,
      string authorsText,
      string title,
      string? subtitle,
      IsbnVo isbnVo
   ) {
      Id = id;
      AuthorsText = authorsText;
      Title = title;
      Subtitle = subtitle;
      IsbnVo = isbnVo;
   }
   
   //--- factory methods -------------------------------------------------------
   // Creates a new Book aggregate and initializes its timestamps.
   // Validation errors are returned as Result failures.
   public static Result<Book> Create(
      Guid id,
      string authorsText,
      string title,
      string? subtitle,
      string isbn,
      DateTime createdAt
   ) {
      string normalizedAuthorsText = AuthorTextParser.NormalizeAuthorsText(
         authorsText: authorsText
      );
      title = title.Trim();
      subtitle = string.IsNullOrWhiteSpace(subtitle) ? null : subtitle.Trim();
      
      IReadOnlyList<string> authorLastnames = AuthorTextParser.ExtractLastnames(
         authorsText: normalizedAuthorsText
      );
      
      // A book needs a valid technical identity.
      if (id == Guid.Empty)
         return Result<Book>.Failure(CatalogErrors.BookIdRequired);
      
      // A book needs at least one author lastname.
      if (authorLastnames.Count == 0)
         return Result<Book>.Failure(CatalogErrors.AuthorsAreRequired);
      
      // A book needs a title.
      if (string.IsNullOrWhiteSpace(title))
         return Result<Book>.Failure(CatalogErrors.TitleIsRequired);

      // Create and validate the ISBN value object.
      var resultIsbn = IsbnVo.Create(isbn);
      if (resultIsbn.IsFailure)
         return Result<Book>.Failure(resultIsbn.Error);
      var isbnVo = resultIsbn.Value;
      
      // Create the aggregate with normalized string values.
      var book = new Book(
         id: id,
         authorsText: normalizedAuthorsText,
         title:  title,
         subtitle: subtitle,
         isbnVo: isbnVo
      );

      // Initialize CreatedAt and UpdatedAt using the inherited lifecycle method.
      var resultCreated = book.Initialize(createdAt);
      if (resultCreated.IsFailure)
         return Result<Book>.Failure(resultCreated.Error);
      
      return Result<Book>.Success(book);
   }

   //--- domain methods --------------------------------------------------------
   // Adds a physical copy of this book (Exemplar) to the aggregate.
   public Result<BookItem> AddBookItem(
      Guid bookItemId,
      string inventoryNumber,
      DateTime updatedAt
   ) {
      inventoryNumber = inventoryNumber.Trim();
      
      // A book item needs a valid technical identity.
      if (bookItemId == Guid.Empty)
         return Result<BookItem>.Failure(CatalogErrors.BookItemIdRequired);

      // Inventory numbers must be unique inside this aggregate.
      // A library-wide uniqueness rule should additionally be checked in the use case.
      if (!string.IsNullOrWhiteSpace(inventoryNumber) &&
          _bookItems.Any(bi => bi.InventoryNumber == inventoryNumber))
         return Result<BookItem>.Failure(CatalogErrors.BookItemAlreadyExists);

      // Create the child entity through its factory method.
      var resultBookItem = BookItem.Create(
         id: bookItemId,
         bookId: Id,
         inventoryNumber: inventoryNumber
      );
      if (resultBookItem.IsFailure)
         return Result<BookItem>.Failure(resultBookItem.Error);
      var bookItem = resultBookItem.Value;

      _bookItems.Add(bookItem);

      // Mark the aggregate as updated.
      var resultUpdated = Touch(updatedAt);
      if (resultUpdated.IsFailure)
         return Result<BookItem>.Failure(resultUpdated.Error);

      return Result<BookItem>.Success(bookItem);
   }
   
   // Deactivate the Book
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
   
   private static string NormalizeAuthorsText(
      string? authorsText
   ) {
      if (string.IsNullOrWhiteSpace(authorsText))
         return string.Empty;

      string[] authorTokens = authorsText.Split(
         separator: ',',
         options: StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
      );

      return string.Join(
         separator: ", ",
         values: authorTokens
      );
   }
}

/*
Lernziele und Didaktik
----------------------

Diese Klasse zeigt Book als Aggregate Root des Catalog-Moduls.
Ein Book repräsentiert das bibliografische Werk, also zum Beispiel
"Clean Code" oder "Domain-Driven Design". Es ist nicht das einzelne
physische Exemplar im Regal.

Book ist ein Aggregate Root, weil es die Konsistenzgrenze für fachlich
zusammengehörende Objekte bildet. Von außen sollen BookItems nicht beliebig
erzeugt oder verändert werden. Stattdessen werden sie über Methoden des
Book-Aggregates verwaltet.

Zum Book-Aggregate gehören aktuell:

* Book als Aggregate Root
* BookItem als untergeordnete Entity für ein physisches Exemplar
* Author als zugeordnetes eigenständiges Aggregate Root
* IsbnVo als Value Object für die ISBN

BookItem ist kein eigenes Aggregate Root. Ein Exemplar gehört fachlich immer
zu genau einem Book. Deshalb wird es über AddBookItem am Book angelegt.
Dadurch kann Book Regeln innerhalb seines Aggregates schützen, zum Beispiel
dass eine InventoryNumber nicht doppelt in derselben Book-Instanz vorkommt.

Die bibliotheksweite Eindeutigkeit einer InventoryNumber kann Book allein
nicht prüfen, weil dafür alle Bücher beziehungsweise alle BookItems bekannt
sein müssen. Diese Prüfung gehört deshalb in den Use Case oder in ein
Repository und wird zusätzlich durch einen Unique Index in der Datenbank
abgesichert.

Die Beziehung zwischen Book und Author ist eine m:n-Beziehung. Da die
Zuordnung aktuell keine eigene fachliche Bedeutung und keine eigenen
Attribute hat, wird keine eigene Domain-Klasse BookAuthor modelliert.
Stattdessen enthält Book eine Liste von Authors. EF Core bildet diese
Beziehung später mit einer Join-Tabelle ab.

Das unterscheidet Book-Author von einem späteren Loan-Modell. Ein Loan ist
nicht nur eine Verbindung zwischen Reader und BookItem, sondern ein eigener
fachlicher Vorgang mit Ausleihdatum, Rückgabefrist, Rückgabedatum und Status.
Loan hätte daher eine eigene fachliche Bedeutung und vermutlich eine eigene
Identität.

Author bleibt dennoch ein eigenes Aggregate Root. Ein Author kann unabhängig
von einem einzelnen Book existieren und mehreren Books zugeordnet werden.
Die Frage "Welche Bücher gibt es von diesem Autor?" wird aber nicht über
eine Books-Liste im Author gelöst, sondern über ein ReadModel oder eine
Query. Dadurch bleibt die Domain-Navigation einfach und die Suchlogik liegt
dort, wo sie hingehört: auf der lesenden Seite.

IsbnVo ist ein Value Object. Es besitzt keine eigene technische Identität,
sondern wird über seinen Wert verglichen. Die Validierung der ISBN liegt
direkt im Value Object und nicht im Controller oder in der Datenbank.

Die Parameter createdAt und updatedAt machen das Domänenmodell testbar.
In Tests kann ein fester UTC-Zeitpunkt oder eine FakeClock verwendet werden.
Dadurch sind CreatedAt und UpdatedAt deterministisch prüfbar und hängen
nicht von DateTime.Now oder der Systemzeit ab.

Initialize(createdAt) wird beim Erzeugen des Aggregates verwendet.
Touch(updatedAt) wird bei fachlichen Änderungen aufgerufen. Dadurch wird
sichtbar, wann ein Aggregate neu entstanden ist und wann es zuletzt geändert
wurde.

Didaktisch zeigt diese Klasse mehrere zentrale DDD-Ideen:

* Aggregate Root als Konsistenzgrenze
* Entity mit technischer Identität
* Value Object ohne eigene Identität
* m:n-Beziehung ohne eigene Domain-Klasse, wenn die Beziehung keine eigene Fachlichkeit hat
* fachliche Methoden statt öffentlicher Setter
* kontrolliertes Erzeugen untergeordneter Entities
* ReadModel für lesende Suchanforderungen
* testbare Zeitsteuerung über UTC-DateTime
 */