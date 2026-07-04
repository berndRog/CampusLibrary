using CampusLibraryApi._2_BuildingBlocks;
using CampusLibraryApi._2_BuildingBlocks._1_Ports;
using CampusLibraryApi._2_BuildingBlocks._2_Application.Utils;
using CampusLibraryApi._3_Core.Catalog._3_Domain.Entities;
using CampusLibraryApi._3_Core.Catalog._3_Domain.Errors;
using CampusLibraryApi._3_Core.Loans._3_Domain.Entities;
using CampusLibraryApi._3_Core.Loans._3_Domain.Errors;
using CampusLibraryApi._3_Core.Loans._3_Domain.ValueObjects;
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

   public AddressVo AddressRitaVo
      => AddressVo.Create("Bibliotheksweg 99", "29556", "Suderburg", "DE").GetValueOrThrow();
   #endregion

   #region -------------- Test Readers (Entities) ------------------------------------------
   public const string Reader1Id = "00000001-0000-0000-0000-000000000000";
   public const string Reader2Id = "00000002-0000-0000-0000-000000000000";
   public const string Reader3Id = "00000003-0000-0000-0000-000000000000";
   public const string Reader4Id = "00000004-0000-0000-0000-000000000000";
   public const string Reader5Id = "00000005-0000-0000-0000-000000000000";
   public const string Reader6Id = "00000006-0000-0000-0000-000000000000";

   public const string ReaderRegisterId = "00000007-0000-0000-0000-000000000000";

   // Demo reader used by the Part 5 CampusLibraryClient DevIdentity.
   public const string RitaReaderId = "00000099-0000-0000-0000-000000000000";

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
      firstname: "Arne",
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

   public Reader RitaReader() => CreateReader(
      id: RitaReaderId,
      firstname: "Rita",
      lastname: "Reader",
      email: "r.reader@library.local",
      addressVo: AddressRitaVo,
      subject: "99000000-0000-0000-0000-000000000000"
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
      Reader1(), Reader2(), Reader3(), Reader4(), Reader5(), Reader6(), RitaReader()
   ];
   #endregion

   #region -------------- Test Books (Aggregates) ------------------------------------------
   public const string Book1Id = "00000001-0000-0000-0000-000000000000";
   public const string Book2Id = "00000002-0000-0000-0000-000000000000";
   public const string Book3Id = "00000003-0000-0000-0000-000000000000";
   public const string Book4Id = "00000004-0000-0000-0000-000000000000";

   public Book Book1() => CreateBook(
      id: Book1Id,
      authorsText: "Robert C. Martin",
      title: "Clean Code",
      subtitle: "A Handbook of Agile Software Craftsmanship",
      isbn: "9780132350884"
   );

   public Book Book2() => CreateBook(
      id: Book2Id,
      authorsText: "Eric Evans",
      title: "Domain-Driven Design",
      subtitle: "Tackling Complexity in the Heart of Software",
      isbn: "9780321125217"
   );

   public Book Book3() => CreateBook(
      id: Book3Id,
      authorsText: "Martin Fowler, Kent Beck",
      title: "Refactoring",
      subtitle: "Improving the Design of Existing Code",
      isbn: "9780201485677"
   );

   public Book Book4() => CreateBook(
      id: Book4Id,
      authorsText: "Erich Gamma, Richard Helm, Ralph Johnson, John Vlissides",
      title: "Design Patterns",
      subtitle: "Elements of Reusable Object-Oriented Software",
      isbn: "9780201633610"
   );

   public IReadOnlyList<Book> Books {
      get {
         var books = new List<Book> {
            Book1(), Book2(), Book3(), Book4()
         };

         AddItemsToBooks(books: books);
         return books;
      }
   }
   #endregion

   #region -------------- Test BookItems ---------------------------------------------------
   public const string BookItem1Id = "00000001-0000-0000-0000-000000000000";
   public const string BookItem2Id = "00000002-0000-0000-0000-000000000000";
   public const string BookItem3Id = "00000003-0000-0000-0000-000000000000";
   public const string BookItem4Id = "00000004-0000-0000-0000-000000000000";
   public const string BookItem5Id = "00000005-0000-0000-0000-000000000000";
   public const string BookItem6Id = "00000006-0000-0000-0000-000000000000";
   #endregion

   #region -------------- Test Loans (Aggregates) ------------------------------------------
   public const string Loan1Id = "00000001-0000-0000-0000-000000000000";
   public const string Loan2Id = "00000002-0000-0000-0000-000000000000";
   public const string Loan3Id = "00000003-0000-0000-0000-000000000000";

   public Loan Loan1() => CreateLoan(
      id: Loan1Id,
      readerId: Reader1Id,
      bookItemId: BookItem1Id,
      borrowedAt: clock.UtcNow.AddDays(-7),
      dueAt: clock.UtcNow.AddDays(7)
   );

   public Loan Loan2() => CreateLoan(
      id: Loan2Id,
      readerId: Reader2Id,
      bookItemId: BookItem3Id,
      borrowedAt: clock.UtcNow.AddDays(-14),
      dueAt: clock.UtcNow
   );

   public Loan Loan3() => CreateLoan(
      id: Loan3Id,
      readerId: Reader3Id,
      bookItemId: BookItem4Id,
      borrowedAt: clock.UtcNow.AddDays(-21),
      dueAt: clock.UtcNow.AddDays(-7)
   );

   public IReadOnlyList<Loan> Loans => [
      Loan1(), Loan2(), Loan3()
   ];
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

   private Book CreateBook(
      string id,
      string authorsText,
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
      // AuthorsText is validated and normalized inside the domain factory.
      var result = Book.Create(
         id: resultId.Value,
         authorsText: authorsText,
         title: title,
         subtitle: subtitle,
         isbn: isbn,
         createdAt: clock.UtcNow
      );

      if (result.IsFailure)
         throw new Exception($"Invalid book in Seed: {title}");

      return result.Value;
   }

   private void AddItemsToBooks(
      List<Book> books
   ) {
      // Book 1: Clean Code
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

      // Book 2: Domain-Driven Design
      AddBookItemToBook(
         book: books[1],
         bookItemId: BookItem3Id,
         inventoryNumber: "CL-BOOK-0003"
      );

      // Book 3: Refactoring
      AddBookItemToBook(
         book: books[2],
         bookItemId: BookItem4Id,
         inventoryNumber: "CL-BOOK-0004"
      );

      // Book 4: Design Patterns
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
         updatedAt: clock.UtcNow.Add(TimeSpan.FromHours(6))
      );

      if (result.IsFailure)
         throw new Exception($"Invalid book item in Seed: {inventoryNumber}");
   }
   
   private Loan CreateLoan(
      string id,
      string readerId,
      string bookItemId,
      DateTime borrowedAt,
      DateTime dueAt
   ) {
      // Resolve the stable seed id of the Loan aggregate.
      var resultLoanId = EntityId.Resolve(
         id,
         LoanErrors.InvalidLoanId
      );
      if (resultLoanId.IsFailure)
         throw new Exception($"Invalid loan id in Seed: {id}");

      // Resolve the referenced Reader id.
      // The Reader aggregate itself is not loaded here.
      // The Loan stores only the foreign key to the Reader.
      var resultReaderId = EntityId.Resolve(
         readerId,
         LoanErrors.InvalidReaderId
      );
      if (resultReaderId.IsFailure)
         throw new Exception($"Invalid reader id in Seed: {readerId}");

      // Resolve the referenced BookItem id.
      // The BookItem aggregate/entity itself is not loaded here.
      // The Loan stores only the foreign key to the concrete physical copy.
      var resultBookItemId = EntityId.Resolve(
         bookItemId,
         LoanErrors.InvalidBookItemId
      );
      if (resultBookItemId.IsFailure)
         throw new Exception($"Invalid book item id in Seed: {bookItemId}");

      
      var resultLoanPeriod = LoanPeriodVo.Create(
          loanDate: borrowedAt,
          dueDate: dueAt
      );
      if (resultLoanPeriod.IsFailure)
         throw new Exception(
            $"Error in LoanPeriod {borrowedAt.ToShortDateString()} " +
            $"{dueAt.ToShortDateString()}"
         );
      var loanVo = resultLoanPeriod.Value;
      
      // Create the Loan aggregate through its factory method.
      var result = Loan.Create(
         id: resultLoanId.Value,
         readerId: resultReaderId.Value,
         bookItemId: resultBookItemId.Value,
         loanPeriodVo:  loanVo
      );

      if (result.IsFailure)
         throw new Exception(
            $"Invalid loan in Seed: Reader={readerId}, BookItem={bookItemId}"
         );

      return result.Value;
   }
   #endregion
}

/*
   Lernziele und Didaktik
   ----------------------
   
   Diese Seed-Klasse stellt stabile Testdaten für das CampusLibrary-Projekt
   bereit. In Teil 1 und Teil 2 wurden nur Reader-Daten benötigt. In Teil 3
   wurde mit Catalog ein zweites Fachmodul ergänzt. In Teil 4 wird mit Loans
   ein drittes Fachmodul eingeführt.
   
   Die Reader-Daten bleiben unverändert. Dadurch kann geprüft werden, dass das
   neue Loans-Modul keine bestehenden Reader-Funktionen beschädigt.
   
   Für Catalog werden Books und BookItems erzeugt. Ein Book beschreibt das
   bibliografische Werk, zum Beispiel "Clean Code" oder "Domain-Driven Design".
   Ein BookItem beschreibt ein konkretes physisches Exemplar dieses Buchs.
   
   Autorinnen und Autoren werden in dieser reduzierten Catalog-Version bewusst
   nicht als eigene Domain Entity modelliert. Stattdessen enthält Book einen
   kommaseparierten Autorentext. Damit bleibt der Catalog-Teil didaktisch
   schlank.
   
   Die Beziehung Book -> BookItem zeigt weiterhin eine echte 1:n-Beziehung.
   Ein Book kann mehrere BookItems besitzen. Die BookItems werden nicht direkt
   erzeugt, sondern über AddBookItem am Book-Aggregate hinzugefügt. Dadurch
   bleibt die fachliche Konsistenzgrenze des Book-Aggregates auch bei Testdaten
   erhalten.
   
   Mit Loans wird nun die eigentliche fachlich wichtige m:n-Beziehung eingeführt:
   Ein Reader kann über die Zeit viele BookItems ausleihen, und ein BookItem kann
   über die Zeit von verschiedenen Readern ausgeliehen werden. Diese Beziehung
   wird aber nicht als reine technische Join-Tabelle modelliert. Stattdessen
   entsteht mit Loan ein eigenes fachliches Objekt.
   
   Loan besitzt eigene fachliche Daten, zum Beispiel Ausleihdatum, Rückgabefrist,
   Rückgabedatum, Verlängerungen und Status. Deshalb ist Loan ein eigenes Modell
   im Loans-Modul und nicht nur eine Navigation zwischen Reader und BookItem.
   
   Die Loan-Seed-Daten verweisen nur über ReaderId und BookItemId auf andere
   Module. Reader und BookItem werden nicht als Objekte in das Loan-Aggregate
   eingebettet. Dadurch bleibt sichtbar, dass die Module fachlich getrennt sind.
   
   Didaktisch wichtig ist die Trennung zwischen Seed-Daten und Fachlogik:
   Der Seed erzeugt Beispieldaten. Die Regeln bleiben aber im Domänenmodell,
   also in Reader, Book, BookItem, IsbnVo und Loan.

   Die stabilen Demo-IDs werden pro Tabelle einfach durchnummeriert. Dadurch
   können HTTP-Tests, Integrationstests und Client-Demos dieselben gut lesbaren
   IDs verwenden. Die Werte dürfen sich zwischen unterschiedlichen Tabellen
   wiederholen, weil jede Tabelle ihren eigenen fachlichen Entity-Typ enthält.

   Rita Reader verwendet bewusst die ID
   00000099-0000-0000-0000-000000000000. Diese ID ist für die Part-5-DevIdentity
   des CampusLibraryClient reserviert und macht sichtbar, dass hier ein
   simulierter angemeldeter Reader verwendet wird.
*/