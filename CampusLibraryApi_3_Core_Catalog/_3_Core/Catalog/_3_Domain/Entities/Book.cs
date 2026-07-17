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
      DateTime updatedAt
   ) {

      // A book item needs a identity == inventary number.
      if (bookItemId == Guid.Empty)
         return Result<BookItem>.Failure(CatalogErrors.BookItemIdRequired);


      // Create the child entity through its factory method.
      var resultBookItem = BookItem.Create(
         id: bookItemId,
         bookId: Id
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

   // Deactivates the Book and removes all physical copies from the aggregate.
   // The application use case must first verify that no current Loan exists
   // for any of these BookItems.
   public Result Deactivate(
      DateTime updatedAt
   ) {
      // A repeated request is idempotent only when the book is already in the
      // complete target state. Older database versions may contain an inactive
      // Book that still owns BookItems. Calling Deactivate again then completes
      // the cleanup after all current loans have been returned.
      if(!IsActive && _bookItems.Count == 0)
         return Result.Success();

      var resultUpdated = Touch(updatedAt);
      if(resultUpdated.IsFailure)
         return Result.Failure(resultUpdated.Error);

      _bookItems.Clear();
      IsActive = false;

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

Book ist das Aggregate Root des Catalog-Moduls. Es beschreibt das
bibliografische Werk; BookItem beschreibt ein konkretes physisches Exemplar.
BookItems werden ausschließlich über das Book-Aggregate hinzugefügt und
entfernt.

Die eindeutige BookItem.Id ist zugleich die fachlich sichtbare
Exemplaridentität. Eine zusätzliche InventoryNumber wird nicht gespeichert.

Autorinnen und Autoren werden in dieser didaktisch reduzierten Variante nicht
als eigene Entities modelliert. Book enthält stattdessen AuthorsText. IsbnVo
bleibt ein Value Object des Book-Aggregates.

Ein vorhandener Loan in einem anderen Bounded Context bedeutet, dass ein
BookItem aktuell ausgeliehen ist. Deshalb fragt der Deaktivierungs-Use-Case
das Loans-Modul über einen BC-Contract, bevor BookItems entfernt werden.
*/
