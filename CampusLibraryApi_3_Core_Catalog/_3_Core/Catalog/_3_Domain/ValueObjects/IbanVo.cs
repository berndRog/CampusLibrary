using CampusLibraryApi._2_BuildingBlocks;
using CampusLibraryApi._3_Core.Catalog._3_Domain.Errors;
namespace CampusLibraryApi._3_Core.Catalog._3_Domain.ValueObjects;

public sealed record IsbnVo {

   public string Value { get; private init; } = string.Empty;

   // EfCore ctor
   private IsbnVo() {
   }
   
   // Domain ctor
   private IsbnVo(string value) {
      Value = value;
   }

   // static factory
   public static Result<IsbnVo> Create(string? value) {

      // The ISBN must be provided by the caller.
      if (string.IsNullOrWhiteSpace(value))
         return Result<IsbnVo>.Failure(CatalogErrors.IsbnIsRequired);

      // Remove formatting characters before validation.
      var normalized = Normalize(value);

      // This teaching example only supports ISBN-13.
      if (normalized.Length != 13)
         return Result<IsbnVo>.Failure(CatalogErrors.IsbnMustHave13Digits);

      // After normalization, the ISBN may only contain digits.
      if (!normalized.All(char.IsDigit))
         return Result<IsbnVo>.Failure(CatalogErrors.IsbnMustContainOnlyDigits);

      // The last digit of an ISBN-13 is a checksum digit.
      if (!HasValidChecksum(normalized))
         return Result<IsbnVo>.Failure(CatalogErrors.IsbnChecksumInvalid);

      return Result<IsbnVo>.Success(new IsbnVo(normalized));
   }

   public static IsbnVo FromPersisted(string value) {
      // EF Core uses this method to rebuild the value object from the database.
      return new IsbnVo(value);
   }

   private static string Normalize(string value) {
      // Common ISBN input formats may contain spaces or hyphens.
      return value
         .Replace("-", string.Empty)
         .Replace(" ", string.Empty)
         .Trim();
   }

   private static bool HasValidChecksum(string isbn) {

      var sum = 0;

      // ISBN-13 uses alternating weights 1 and 3 for the first 12 digits.
      for (var i = 0; i < 12; i++) {
         var digit = isbn[i] - '0';

         sum += i % 2 == 0
            ? digit
            : digit * 3;
      }

      // The calculated checksum must match the thirteenth digit.
      var checkDigit = isbn[12] - '0';
      var expectedCheckDigit = (10 - sum % 10) % 10;

      return checkDigit == expectedCheckDigit;
   }

   public override string ToString() => Value;
}

/*
Lernziele und Didaktik
----------------------

Dieses Value Object zeigt, wie fachliche Regeln direkt im Domänenmodell
gekapselt werden können. Eine ISBN ist nicht einfach nur ein string,
sondern ein fachlicher Wert mit eigenen Gültigkeitsregeln.

Die Studierenden erkennen hier den Unterschied zwischen primitiven
Datentypen und fachlichen Value Objects. Durch die Factory-Methode Create
wird verhindert, dass ungültige ISBN-Werte im normalen Anwendungscode
erzeugt werden.

Der Typ ist als sealed record modelliert, weil Value Objects über ihren
Wert und nicht über eine technische Identität verglichen werden. Zwei
IsbnVo-Objekte mit demselben Value gelten fachlich als gleich.

Die Methode FromPersisted ist bewusst von Create getrennt. Create wird für
neue Benutzereingaben verwendet und validiert die Fachregeln. FromPersisted
dient dem Wiederherstellen bereits gespeicherter Werte aus der Datenbank.

Didaktisch wichtig ist außerdem die Normalisierung: Benutzer dürfen eine
ISBN mit Bindestrichen oder Leerzeichen eingeben, intern wird aber eine
einheitliche Darstellung gespeichert. Dadurch bleibt das Domänenmodell
robust und die Persistenz einfacher.

In Teil 3 des CampusLibrary-Projekts ist IsbnVo ein gutes erstes Beispiel
für ein Value Object im neuen Catalog-Modul. Es ergänzt die Entities Book,
Author, BookAuthor und BookItem um eine fachlich bedeutsame Regel, ohne
bereits Controller, Datenbankzugriffe oder UseCases einzubeziehen.
*/