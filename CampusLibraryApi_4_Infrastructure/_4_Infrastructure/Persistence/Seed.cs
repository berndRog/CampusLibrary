using CampusLibraryApi._2_BuildingBlocks._1_Ports;
using CampusLibraryApi._2_BuildingBlocks._2_Application.Utils;
using CampusLibraryApi._3_Core.Catalog._3_Domain.Entities;
using CampusLibraryApi._3_Core.Catalog._3_Domain.Errors;
using CampusLibraryApi._3_Core.Readers._3_Domain.Entities;
using CampusLibraryApi._3_Core.Readers._3_Domain.Errors;
using CampusLibraryApi._3_Core.Readers._3_Domain.ValueObjects;

namespace CampusLibraryApi._4_Infrastructure.Persistence;

public sealed class Seed(
   IClock clock
) {
   #region -------------- Test Addresses (Value Objects) -------------------------------------
   public AddressVo Address1Vo
      => AddressVo.Create("Hauptstr. 23", "29556", "Suderburg", "DE").GetValueOrThrow();

   public AddressVo Address2Vo
      => AddressVo.Create("Hauptstr. 23", "29556", "Suderburg", "DE").GetValueOrThrow();

   public AddressVo Address3Vo
      => AddressVo.Create("Neuperverstraße. 29", "29410", "Salzwedel").GetValueOrThrow();

   public AddressVo Address4Vo
      => AddressVo.Create("Schillerstr. 1", "30123", "Hannover", "DE").GetValueOrThrow();

   public AddressVo Address5Vo
      => AddressVo.Create("Berliner Platz 8", "29614", "Soltau", "DE").GetValueOrThrow();

   public AddressVo Address6Vo
      => AddressVo.Create("Allertalweg. 2", "29227", "Celle", "DE").GetValueOrThrow();

   public AddressVo AddressRegVo
      => AddressVo.Create("Am Markt 14", "04109", "Leipzig", "DE").GetValueOrThrow();
   #endregion

   #region -------------- Test Readers (Entities) ------------------------------------------
   private const string Reader1Id = "00000001-0000-0000-0000-000000000000";
   private const string Reader2Id = "00000002-0000-0000-0000-000000000000";
   private const string Reader3Id = "00000003-0000-0000-0000-000000000000";
   private const string Reader4Id = "00000004-0000-0000-0000-000000000000";
   private const string Reader5Id = "00000005-0000-0000-0000-000000000000";
   private const string Reader6Id = "00000006-0000-0000-0000-000000000000";

   private const string ReaderRegisterId = "00000007-0000-0000-0000-000000000000";

   public Reader Reader1() => CreateReader(
      id: Reader1Id,
      firstname: "Erika",
      lastname: "Mustermann",
      email: "erika.mustermann@t-online.de",
      addressVo: Address1Vo,
      subject: "a00090ad-d9df-486a-8757-4a649e26a54e"
   );

   public Reader Reader2() => CreateReader(
      id: Reader2Id,
      firstname: "Max",
      lastname: "Mustermann",
      email: "max.mustermann@gmail.com",
      addressVo: Address2Vo,
      subject: "b0000640-161e-4228-9729-d6b142C2dfad"
   );

   public Reader Reader3() => CreateReader(
      id: Reader3Id,
      firstname: "Arno",
      lastname: "Arndt",
      email: "a.arndt@t-online.de",
      addressVo: Address3Vo,
      subject: "c0004e61-ba7a-4d2a-977f-766b42bb79a9"
   );

   public Reader Reader4() => CreateReader(
      id: Reader4Id,
      firstname: "Benno",
      lastname: "Bauer",
      email: "b.bauer@gmail.com",
      addressVo: Address4Vo,
      subject: "d0024ab-43c5-4c64-872d-6ca05f66756b"
   );

   public Reader Reader5() => CreateReader(
      id: Reader5Id,
      firstname: "Christine",
      lastname: "Conrad",
      email: "c.conrad@gmx.de",
      addressVo: Address5Vo,
      subject: "e00050fb-a381-4e3f-a44b-81ffa7610b72"
   );

   public Reader Reader6() => CreateReader(
      id: Reader6Id,
      firstname: "Dana",
      lastname: "Deppe",
      email: "d.deppe@icloud.com",
      addressVo: Address6Vo,
      subject: "f00060A1-1381-efab-1440-71fc17630172"
   );

   public Reader ReaderRegister() => CreateReader(
      id: ReaderRegisterId,
      firstname: "Edgar",
      lastname: "Engel",
      email: "e.engel@freenet.de",
      addressVo: AddressRegVo,
      subject: "70000000-0007-0000-0000-000000000000"
   );

   public IReadOnlyList<Reader> Readers => [
      Reader1(), Reader2(), Reader3(), Reader4(), Reader5(), Reader6()
   ];
   #endregion

   #region -------------- Test Authors (Aggregates) ------------------------------------------
   public const string Author1Id = "a0000001-0000-0000-0000-000000000000";
   public const string Author2Id = "a0000002-0000-0000-0000-000000000000";
   public const string Author3Id = "a0000003-0000-0000-0000-000000000000";
   public const string Author4Id = "a0000004-0000-0000-0000-000000000000";
   public const string Author5Id = "a0000005-0000-0000-0000-000000000000";

   public Author Author1() => CreateAuthor(
      id: Author1Id,
      firstname: "Robert C.",
      lastname: "Martin"
   );

   public Author Author2() => CreateAuthor(
      id: Author2Id,
      firstname: "Eric",
      lastname: "Evans"
   );

   public Author Author3() => CreateAuthor(
      id: Author3Id,
      firstname: "Martin",
      lastname: "Fowler"
   );

   public Author Author4() => CreateAuthor(
      id: Author4Id,
      firstname: "Erich",
      lastname: "Gamma"
   );

   public Author Author5() => CreateAuthor(
      id: Author5Id,
      firstname: "Kent",
      lastname: "Beck"
   );

   public IReadOnlyList<Author> Authors => [
      Author1(), Author2(), Author3(), Author4(), Author5()
   ];
   #endregion

   #region -------------- Test Books (Aggregates) ------------------------------------------
   public const string Book1Id = "b0000001-0000-0000-0000-000000000000";
   public const string Book2Id = "b0000002-0000-0000-0000-000000000000";
   public const string Book3Id = "b0000003-0000-0000-0000-000000000000";
   public const string Book4Id = "b0000004-0000-0000-0000-000000000000";

   public Book Book1() => CreateBook(
      id: Book1Id,
      title: "Clean Code",
      subtitle: "A Handbook of Agile Software Craftsmanship",
      isbn: "9780132350884"
   );

   public Book Book2() => CreateBook(
      id: Book2Id,
      title: "Domain-Driven Design",
      subtitle: "Tackling Complexity in the Heart of Software",
      isbn: "9780321125217"
   );

   public Book Book3() => CreateBook(
      id: Book3Id,
      title: "Refactoring",
      subtitle: "Improving the Design of Existing Code",
      isbn: "9780201485677"
   );

   public Book Book4() => CreateBook(
      id: Book4Id,
      title: "Design Patterns",
      subtitle: "Elements of Reusable Object-Oriented Software",
      isbn: "9780201633610"
   );

   // Convenience property for simple tests.
   // For EF seeding prefer BooksWithAuthors(authors), so the same Author instances
   // are used for Authors and for the Book.Authors navigation.
   public IReadOnlyList<Book> Books {
      get {
         var authors = Authors;
         return BooksWithAuthors(authors);
      }
   }

   // Use this method when Authors and Books are added to the same DbContext.
   // This avoids creating duplicate Author instances with the same keys.
   public IReadOnlyList<Book> BooksWithAuthors(
      IReadOnlyList<Author> authors
   ) {
      var books = new List<Book> {
         Book1(), Book2(), Book3(), Book4()
      };
      AddAuthorsAndItemsToBooks(
         books: books,
         authors: authors
      );

      return books;
   }
   #endregion
   


   #region -------------- Test Bookitems  ---------------------------------------------------
   public const string BookItem1Id = "be000001-0000-0000-0000-000000000000";
   public const string BookItem2Id = "be000002-0000-0000-0000-000000000000";
   public const string BookItem3Id = "be000003-0000-0000-0000-000000000000";
   public const string BookItem4Id = "be000004-0000-0000-0000-000000000000";
   public const string BookItem5Id = "be000005-0000-0000-0000-000000000000";
   public const string BookItem6Id = "be000006-0000-0000-0000-000000000000";
   #endregion
   
   #region -------------- Test Bookitems  ---------------------------------------------------
   public const string BookAuthor1Id = "ba000001-0000-0000-0000-000000000000";
   public const string BookAuthor2Id = "ba000002-0000-0000-0000-000000000000";
   public const string BookAuthor3Id = "ba000003-0000-0000-0000-000000000000";
   #endregion
   
   
   #region -------------- Helper Methods ----------------------------------------------------
   private Reader CreateReader(
      string id,
      string firstname,
      string lastname,
      string email,
      AddressVo addressVo,
      string subject
   ) {
      // Create and validate the email value object.
      var resultEmail = EmailVo.Create(email);
      if (resultEmail.IsFailure)
         throw new Exception($"Invalid email in Seed: {email}");
      var emailVo = resultEmail.Value;

      // Resolve the stable seed id.
      var resultId = EntityId.Resolve(
         id,
         ReaderErrors.InvalidId
      );

      if (resultId.IsFailure)
         throw new Exception($"Invalid reader id in Seed: {id}");

      // Create the Reader aggregate through its factory method.
      var result = Reader.Create(
         id: resultId.Value,
         firstname: firstname,
         lastname: lastname,
         subject: subject,
         emailVo: emailVo,
         addressVo: addressVo,
         createdAt: clock.UtcNow
      );

      if (result.IsFailure)
         throw new Exception($"Invalid reader in Seed: {firstname} {lastname}");

      return result.Value;
   }

   private Author CreateAuthor(
      string id,
      string firstname,
      string lastname
   ) {
      // Resolve the stable seed id.
      var resultId = EntityId.Resolve(
         id,
         CatalogErrors.InvalidAuthorId
      );
      if (resultId.IsFailure)
         throw new Exception($"Invalid author id in Seed: {id}");

      // Create the Author aggregate through its factory method.
      var result = Author.Create(
         id: resultId.Value,
         firstname: firstname,
         lastname: lastname,
         createdAt: clock.UtcNow
      );

      if (result.IsFailure)
         throw new Exception($"Invalid author in Seed: {firstname} {lastname}");

      return result.Value;
   }

   private Book CreateBook(
      string id,
      string title,
      string? subtitle,
      string isbn
   ) {
      // Resolve the stable seed id.
      var resultId = EntityId.Resolve(
         id,
         CatalogErrors.InvalidBookId
      );
      if (resultId.IsFailure)
         throw new Exception($"Invalid book id in Seed: {id}");

      // Create the Book aggregate through its factory method.
      var result = Book.Create(
         id: resultId.Value,
         title: title,
         subtitle: subtitle,
         isbn: isbn,
         createdAt: clock.UtcNow
      );

      if (result.IsFailure)
         throw new Exception($"Invalid book in Seed: {title}");

      return result.Value;
   }

   private void AddAuthorsAndItemsToBooks(
      List<Book> books,
      IReadOnlyList<Author> authors
   ) {
      // Book 1: Clean Code -> Robert C. Martin
      AddAuthorToBook(
         book: books[0],
         authors: authors,
         authorId: Author1Id
      );

      AddBookItemToBook(
         book: books[0],
         bookItemId: BookItem1Id,
         inventoryNumber: "CL-BOOK-0001"
      );

      AddBookItemToBook(
         book: books[0],
         bookItemId: BookItem2Id,
         inventoryNumber: "CL-BOOK-0002"
      );

      // Book 2: Domain-Driven Design -> Eric Evans
      AddAuthorToBook(
         book: books[1],
         authors: authors,
         authorId: Author2Id
      );

      AddBookItemToBook(
         book: books[1],
         bookItemId: BookItem3Id,
         inventoryNumber: "CL-BOOK-0003"
      );

      // Book 3: Refactoring -> Martin Fowler, Kent Beck
      AddAuthorToBook(
         book: books[2],
         authors: authors,
         authorId: Author3Id
      );

      AddAuthorToBook(
         book: books[2],
         authors: authors,
         authorId: Author5Id
      );

      AddBookItemToBook(
         book: books[2],
         bookItemId: BookItem4Id,
         inventoryNumber: "CL-BOOK-0004"
      );

      // Book 4: Design Patterns -> Erich Gamma, Martin Fowler
      AddAuthorToBook(
         book: books[3],
         authors: authors,
         authorId: Author4Id
      );

      AddAuthorToBook(
         book: books[3],
         authors: authors,
         authorId: Author3Id
      );

      AddBookItemToBook(
         book: books[3],
         bookItemId: BookItem5Id,
         inventoryNumber: "CL-BOOK-0005"
      );

      AddBookItemToBook(
         book: books[3],
         bookItemId: BookItem6Id,
         inventoryNumber: "CL-BOOK-0006"
      );
   }

   private void AddAuthorToBook(
      Book book,
      IReadOnlyList<Author> authors,
      string authorId
   ) {
      // Resolve the id of the assigned Author.
      var resultAuthorId = EntityId.Resolve(
         authorId,
         CatalogErrors.InvalidAuthorId
      );
      if (resultAuthorId.IsFailure)
         throw new Exception($"Invalid author id in Seed: {authorId}");

      var author = authors.SingleOrDefault(a => a.Id == resultAuthorId.Value);

      if (author is null)
         throw new Exception($"Author not found in Seed: {authorId}");

      // The Book aggregate controls the author assignment.
      // EF Core maps the m:n relationship to a join table behind the scenes.
      var result = book.AssignAuthor(
         author: author,
         updatedAt: clock.UtcNow.Add(TimeSpan.FromHours(6))
      );

      if (result.IsFailure)
         throw new Exception(
            $"Invalid author assignment in Seed: Book={book.Id}, Author={authorId}"
         );
   }

   private void AddBookItemToBook(
      Book book,
      string bookItemId,
      string inventoryNumber
   ) {
      // Resolve the id of the BookItem entity.
      var resultBookItemId = EntityId.Resolve(
         bookItemId,
         CatalogErrors.InvalidBookItemId
      );
      if (resultBookItemId.IsFailure)
         throw new Exception($"Invalid book item id in Seed: {bookItemId}");

      // The Book aggregate controls its BookItems.
      var result = book.AddBookItem(
         bookItemId: resultBookItemId.Value,
         inventoryNumber: inventoryNumber,
         updatedAt: clock.UtcNow.Add(TimeSpan.FromHours(6))
      );

      if (result.IsFailure)
         throw new Exception($"Invalid book item in Seed: {inventoryNumber}");
   }
   #endregion
}

/*
Lernziele und Didaktik
----------------------

Diese Seed-Klasse stellt stabile Testdaten für das CampusLibrary-Projekt
bereit. In Teil 1 und Teil 2 wurden nur Reader-Daten benötigt. In Teil 3
wird mit Catalog ein zweites Fachmodul ergänzt.

Die Reader-Daten bleiben unverändert. Dadurch kann geprüft werden, dass das
neue Catalog-Modul keine bestehenden Tests oder bestehende Funktionalität
beschädigt.

Für Catalog werden Authors und Books getrennt erzeugt. Damit wird sichtbar,
dass Author ein eigenständiges fachliches Objekt ist, während Book das zentrale
Aggregate im Catalog-Modul bildet.

Die Beziehung Book -> BookItem zeigt eine 1:n-Beziehung. Ein Book beschreibt
das bibliografische Werk, während BookItem ein konkretes Exemplar dieses Buchs
repräsentiert.

Die Beziehung Book <-> Author zeigt eine m:n-Beziehung. Da diese Zuordnung
aktuell keine eigene fachliche Bedeutung und keine eigenen Attribute besitzt,
wird keine eigene Domain-Klasse BookAuthor modelliert. Stattdessen enthält
Book direkt eine Liste von Authors. EF Core bildet daraus später eine
Join-Tabelle.

BookItems werden nicht direkt erzeugt, sondern über AddBookItem am
Book-Aggregate hinzugefügt. Dadurch bleibt die fachliche Konsistenzgrenze des
Aggregates auch bei Testdaten erhalten.

Authors werden ebenfalls nicht über eine BookAuthor-Klasse verbunden. Die
Methode AssignAuthor am Book-Aggregate stellt sicher, dass derselbe Author
nicht mehrfach demselben Book zugeordnet wird.

Didaktisch wichtig ist die Unterscheidung zwischen einer reinen technischen
m:n-Verbindung und einem fachlichen Vorgang. Book-Author ist hier nur eine
Zuordnung. Ein späterer Loan zwischen Reader und BookItem wäre dagegen ein
eigener fachlicher Vorgang mit Ausleihdatum, Rückgabefrist, Rückgabedatum und
Status.

Die Methode BooksWithAuthors(authors) ist beim EF-Seeding hilfreich, weil sie
dieselben Author-Instanzen verwendet, die auch separat in den DbContext
eingefügt werden können. Dadurch entstehen keine doppelten Author-Objekte mit
derselben Id im ChangeTracker.

Didaktisch wichtig ist die Trennung zwischen Seed-Daten und Fachlogik:
Der Seed erzeugt Beispieldaten. Die Regeln bleiben aber im Domänenmodell,
also in Book, Author, BookItem und IsbnVo.
*/