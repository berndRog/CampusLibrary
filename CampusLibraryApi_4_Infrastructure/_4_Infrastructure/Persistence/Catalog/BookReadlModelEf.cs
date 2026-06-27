using System.Linq.Expressions;
using CampusLibraryApi._2_BuildingBlocks;
using CampusLibraryApi._3_Core.Catalog._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Catalog._2_Application.Dtos;
using CampusLibraryApi._3_Core.Catalog._2_Application.Enums;
using CampusLibraryApi._3_Core.Catalog._3_Domain.Entities;
using CampusLibraryApi._3_Core.Catalog._3_Domain.Enums;
using CampusLibraryApi._3_Core.Catalog._3_Domain.Errors;
using CampusLibraryApi._3_Core.Catalog._3_Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
namespace CampusLibraryApi._4_Infrastructure.Persistence.Catalog;

// EF Core implementation of the book read model.
// Read models are used for query operations and project database data
// directly into DTOs. They do not return domain aggregates.
internal sealed class BookReadModelEf(
   ICatalogDbContext bookDbContext
) : IBookReadModel {

   // Finds one book by id.
   // By default, inactive books are filtered out.
   public async Task<Result<BookDetailDto>> FindByIdAsync(
      Guid id,
      bool includeInactive = false,
      CancellationToken ct = default
   ) {
      BookDetailDto? dto = await bookDbContext.Books
         .AsNoTracking()
         .Where(book => book.Id == id)
         .Where(book => includeInactive || book.IsActive)
         .Select(BookToDetailDto)
         .FirstOrDefaultAsync(ct);

      if(dto is null)
         return Result<BookDetailDto>.Failure(CatalogErrors.BookNotFound);

      return Result<BookDetailDto>.Success(dto);
   }

   // Returns books as list item DTOs.
   // By default, inactive books are filtered out.
   public async Task<Result<IReadOnlyList<BookListItemDto>>> SelectAllAsync(
      bool includeInactive = false,
      CancellationToken ct = default
   ) {
      List<BookListItemDto> books = await bookDbContext.Books
         .AsNoTracking()
         .Where(book => includeInactive || book.IsActive)
         .OrderBy(book => book.Title)
         .ThenBy(book => book.Subtitle)
         .Select(BookToListItemDto)
         .ToListAsync(ct);

      return Result<IReadOnlyList<BookListItemDto>>.Success(books);
   }

   // Searches books by one selected search field.
   // By default, inactive books are filtered out.
   public async Task<Result<IReadOnlyList<BookListItemDto>>> SearchAsync(
      BookSearchDto search,
      bool includeInactive = false,
      CancellationToken ct = default
   ) {
      string searchText = search.SearchText
         .Trim();

      if(string.IsNullOrWhiteSpace(searchText))
         return Result<IReadOnlyList<BookListItemDto>>.Success([]);

      IQueryable<Book> query = bookDbContext.Books
         .AsNoTracking()
         .Where(book => includeInactive || book.IsActive);

      if(search.SearchField == BookSearchField.Title) {
         string pattern = $"%{searchText}%";

         List<BookListItemDto> booksByTitle = await query
            .Where(book => EF.Functions.Like(book.Title, pattern))
            .OrderBy(book => book.Title)
            .ThenBy(book => book.Subtitle)
            .Select(BookToListItemDto)
            .ToListAsync(ct);

         return Result<IReadOnlyList<BookListItemDto>>.Success(booksByTitle);
      }

      if(search.SearchField == BookSearchField.Isbn) {
         string normalizedIsbn = searchText
            .Replace("-", string.Empty)
            .Replace(" ", string.Empty);

         var isbnVo = IsbnVo.FromPersisted(
            value: normalizedIsbn
         );

         List<BookListItemDto> booksByIsbn = await query
            .Where(book => book.IsbnVo == isbnVo)
            .OrderBy(book => book.Title)
            .ThenBy(book => book.Subtitle)
            .Select(BookToListItemDto)
            .ToListAsync(ct);

         return Result<IReadOnlyList<BookListItemDto>>.Success(booksByIsbn);
      }

      if(search.SearchField == BookSearchField.AuthorLastName) {
         List<Book> books = await query
            .Include(book => book.BookItems)
            .ToListAsync(ct);

         List<BookListItemDto> booksByAuthorLastName = books
            .Where(book => ContainsAuthorLastName(
               authorsText: book.AuthorsText,
               searchText: searchText
            ))
            .OrderBy(book => book.Title)
            .ThenBy(book => book.Subtitle)
            .Select(ToListItemDto)
            .ToList();

         return Result<IReadOnlyList<BookListItemDto>>.Success(booksByAuthorLastName);
      }

      return Result<IReadOnlyList<BookListItemDto>>.Success([]);
   }

   // DTO projection used by EF Core for list views.
   // AvailableItems counts only physical copies with status Available.
   // TotalItems counts all physical copies, including unavailable, lost or damaged items.
   private static readonly Expression<Func<Book, BookListItemDto>> BookToListItemDto =
      book => new BookListItemDto(
         Id: book.Id,
         AuthorsText: book.AuthorsText,
         Title: book.Title,
         Subtitle: book.Subtitle,
         Isbn: book.IsbnVo.Value,
         TotalItems: book.BookItems.Count,
         AvailableItems: book.BookItems.Count(
            item => item.Status == BookItemStatus.Available
         ),
         IsActive: book.IsActive
      );

   // DTO projection used by EF Core for detail views.
   // The detail view includes the physical book items.
   private static readonly Expression<Func<Book, BookDetailDto>> BookToDetailDto =
      book => new BookDetailDto(
         // CAUTION: Only positional arguments are working
         // Id:
         book.Id,
         // AuthorsText:
         book.AuthorsText,
         // Title:
         book.Title,
         // Subtitle:
         book.Subtitle,
         // Isbn:
         book.IsbnVo.Value,
         // BookItems:
         book.BookItems
            .OrderBy(item => item.InventoryNumber)
            .Select(item => new BookItemDto(
               // Id: 
               item.Id,
               // BookId: 
               item.BookId,
               // InventoryNumber:
               item.InventoryNumber,
               // Status:
               (int) item.Status
            ))
            .ToList(),
         // TotalItems:
         book.BookItems.Count,
         // AvailableItems:
         book.BookItems.Count(
            item => item.Status == BookItemStatus.Available
         ),
         // IsActive:
         book.IsActive
      );

   // In-memory mapping used for author-last-name searches.
   // This path is used because the simplified author model stores authors
   // as comma-separated text and the last-name extraction is not a clean SQL query.
   private static BookListItemDto ToListItemDto(
      Book book
   ) => new(
      Id: 
      book.Id,
      AuthorsText: 
      book.AuthorsText,
      Title: 
      book.Title,
      Subtitle: book.Subtitle,
      Isbn: book.IsbnVo.Value,
      IsActive: book.IsActive,
      TotalItems: book.BookItems.Count,
      AvailableItems: book.BookItems.Count(
         item => item.Status == BookItemStatus.Available
      )
   );

   // Checks whether the comma-separated author text contains a matching last name.
   //
   // Example:
   // "Martin Fowler, Eric Evans"
   //
   // Author parts:
   // - Martin Fowler -> Fowler
   // - Eric Evans    -> Evans
   private static bool ContainsAuthorLastName(
      string authorsText,
      string searchText
   ) {
      string normalizedSearchText = searchText
         .Trim()
         .ToLowerInvariant();

      if(string.IsNullOrWhiteSpace(normalizedSearchText))
         return false;

      return authorsText
         .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
         .Select(ExtractLastName)
         .Any(lastName => lastName.Contains(
            normalizedSearchText,
            StringComparison.OrdinalIgnoreCase
         ));
   }

   // Extracts the last token from one author text.
   //
   // Example:
   // "Martin Fowler" -> "Fowler"
   private static string ExtractLastName(
      string authorText
   ) {
      string[] parts = authorText
         .Trim()
         .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

      if(parts.Length == 0)
         return string.Empty;

      return parts[^1];
   }
}

/*
Didaktik
--------

Diese Klasse implementiert das ReadModel des Catalog-Moduls für Book-Abfragen.

Das ReadModel ist die Query-Seite der Anwendung. Es lädt keine Aggregates für
schreibende Fachlogik, sondern projiziert Daten direkt in DTOs für Controller,
Swagger, Tests und spätere Clients.

Der Parameter includeInactive steuert die Sicht auf Books:

   includeInactive = false

liefert nur aktive Books.

   includeInactive = true

liefert aktive und inaktive Books.

Diese Regel bezieht sich ausschließlich auf Book.IsActive. Sie bezieht sich
nicht auf BookItemStatus.

BookItemStatus beschreibt den fachlichen Zustand eines physischen Exemplars:

- Available
- Unavailable
- Lost
- Damaged

Für Listen und Details werden zwei Zählwerte gebildet:

TotalItems
- zählt alle BookItems eines Books

AvailableItems
- zählt nur BookItems mit Status Available

Dadurch bleibt sichtbar:

- Book.IsActive steuert die Sichtbarkeit eines Buchs im Katalog.
- BookItemStatus steuert die Verfügbarkeit eines einzelnen Exemplars.

Die Suche nach AuthorLastName ist bewusst ein Sonderfall. Da es in dieser
reduzierten Catalog-Version keine Author-Entity mehr gibt, stehen Autoren als
kommaseparierter Text in Book.AuthorsText. Die Nachnamenlogik wird deshalb
nach dem Laden der passenden Book-Grundmenge im Speicher ausgeführt.

Lernziele
---------

- ReadModel als Query-Seite verstehen
- Book.IsActive von BookItemStatus unterscheiden
- includeInactive als Sichtbarkeitsparameter modellieren
- DTO-Projektionen mit EF Core einsetzen
- verfügbare Exemplare über BookItemStatus.Available zählen
- vereinfachte Autorensuche ohne Author-Entity nachvollziehen
*/
/*
using CampusLibraryApi._2_BuildingBlocks;
using CampusLibraryApi._3_Core.Catalog._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Catalog._2_Application.Dtos;
using CampusLibraryApi._3_Core.Catalog._2_Application.Enums;
using CampusLibraryApi._3_Core.Catalog._2_Application.Mappings;
using CampusLibraryApi._3_Core.Catalog._3_Domain.Entities;
using CampusLibraryApi._3_Core.Catalog._3_Domain.Enums;
using CampusLibraryApi._3_Core.Catalog._3_Domain.Errors;
using CampusLibraryApi._3_Core.Catalog._3_Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace CampusLibraryApi._4_Infrastructure.Persistence.Catalog;

internal sealed class BookReadModelEf(
   ICatalogDbContext dbContext
) : IBookReadModel {

   public async Task<Result<BookDetailDto>> FindByIdAsync(
      Guid id,
      CancellationToken ct = default
   ) {
      if (id == Guid.Empty)
         return Result<BookDetailDto>.Failure(CatalogErrors.InvalidBookId);

      var book = await dbContext.Books
         .AsNoTracking()
         .Include(b => b.BookItems)
         .AsSplitQuery()
         .SingleOrDefaultAsync(
            predicate: b => b.Id == id && b.IsActive,
            cancellationToken: ct
         );

      if (book is null)
         return Result<BookDetailDto>.Failure(CatalogErrors.BookNotFound);

      return Result<BookDetailDto>.Success(
         ToBookDetailDto(
            book: book
         )
      );
   }

   public async Task<Result<IReadOnlyList<BookListItemDto>>> SelectAllAsync(
      CancellationToken ct = default
   ) {
      var books = await dbContext.Books
         .AsNoTracking()
         .Include(b => b.BookItems)
         .AsSplitQuery()
         .Where(b => b.IsActive)
         .OrderBy(b => b.Title)
         .ToListAsync(
            cancellationToken: ct
         );

      var bookDtos = books
         .Select(ToBookListItemDto)
         .ToList();

      return Result<IReadOnlyList<BookListItemDto>>.Success(bookDtos);
   }

   public async Task<Result<IReadOnlyList<BookListItemDto>>> SearchAsync(
      BookSearchDto search,
      CancellationToken ct = default
   ) {
      if (string.IsNullOrWhiteSpace(search.SearchText))
         return Result<IReadOnlyList<BookListItemDto>>.Success([]);

      var normalizedSearchText = search.SearchText.Trim();
      var pattern = $"%{normalizedSearchText}%";

      IQueryable<Book> query = dbContext.Books
         .AsNoTracking()
         .Include(b => b.BookItems)
         .AsSplitQuery()
         .Where(b => b.IsActive);

      if (search.SearchField == BookSearchField.AuthorLastName) {

         var candidates = await query
            .Where(b => EF.Functions.Like(
               b.AuthorsText,
               pattern
            ))
            .OrderBy(b => b.Title)
            .ToListAsync(
               cancellationToken: ct
            );

         var authorMatches = candidates
            .Where(b => ContainsAuthorLastname(
               authorsText: b.AuthorsText,
               searchText: normalizedSearchText
            ))
            .Select(ToBookListItemDto)
            .ToList();

         return Result<IReadOnlyList<BookListItemDto>>.Success(authorMatches);
      }

      query = search.SearchField switch {
         BookSearchField.Title =>
            query.Where(b => EF.Functions.Like(
               b.Title,
               pattern
            )),

         BookSearchField.Isbn =>
            query.Where(b =>
               b.IsbnVo == IsbnVo.FromPersisted(normalizedSearchText)),

         _ =>
            query.Where(_ => false)
      };

      var books = await query
         .OrderBy(b => b.Title)
         .ToListAsync(
            cancellationToken: ct
         );

      var bookDtos = books
         .Select(ToBookListItemDto)
         .ToList();

      return Result<IReadOnlyList<BookListItemDto>>.Success(bookDtos);
   }

   private static BookListItemDto ToBookListItemDto(
      Book book
   ) =>
      new(
         Id: book.Id,
         Title: book.Title,
         Subtitle: book.Subtitle,
         Isbn: book.IsbnVo.Value,
         AuthorsText: book.AuthorsText,
         TotalBookItems: book.BookItems.Count,
         AvailableBookItems: book.BookItems.Count(bi =>
            bi.Status == BookItemStatus.Available)
      );

   private static BookDetailDto ToBookDetailDto(
      Book book
   ) =>
      new(
         Id: book.Id,
         Title: book.Title,
         Subtitle: book.Subtitle,
         Isbn: book.IsbnVo.Value,
         AuthorsText: book.AuthorsText,
         BookItems: book.BookItems
            .OrderBy(bi => bi.InventoryNumber)
            .Select(bookItem => bookItem.ToBookItemDto())
            .ToList(),
         TotalBookItems: book.BookItems.Count,
         AvailableBookItems: book.BookItems.Count(bi =>
            bi.Status == BookItemStatus.Available),
         IsActive: book.IsActive,
         CreatedAt: book.CreatedAt,
         UpdatedAt: book.UpdatedAt
      );

   private static bool ContainsAuthorLastname(
      string authorsText,
      string searchText
   ) {
      if (string.IsNullOrWhiteSpace(authorsText))
         return false;

      if (string.IsNullOrWhiteSpace(searchText))
         return false;

      var normalizedSearchText = Normalize(
         value: searchText
      );

      return ExtractAuthorLastnames(
            authorsText: authorsText
         )
         .Any(lastname =>
            Normalize(
               value: lastname
            ).Contains(normalizedSearchText));
   }

   private static IEnumerable<string> ExtractAuthorLastnames(
      string authorsText
   ) {
      if (string.IsNullOrWhiteSpace(authorsText))
         yield break;

      var authorTokens = authorsText.Split(
         separator: ',',
         options: StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
      );

      foreach (var authorToken in authorTokens) {

         var nameParts = authorToken.Split(
            separator: ' ',
            options: StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
         );

         if (nameParts.Length == 0)
            continue;

         yield return nameParts[^1];
      }
   }

   private static string Normalize(
      string value
   ) =>
      value.Trim().ToLowerInvariant();
}

/*
Lernziele und Didaktik
----------------------

Dieses ReadModel gehört zur Query-Seite des Catalog-Moduls.

Es liefert keine Book-Aggregates an die Web-Schicht zurück, sondern DTOs, die
für Anzeige und Suche optimiert sind. Dadurch bleibt das Domain-Modell geschützt
und wird nicht direkt an Controller oder Clients weitergegeben.

FindByIdAsync liefert ein BookDetailDto für Detailansichten.
SelectAllAsync und SearchAsync liefern BookListItemDto für Listen und
Trefferanzeigen.

Autorinnen und Autoren werden in dieser reduzierten Catalog-Version nicht als
eigene Domain Entity modelliert. Stattdessen enthält Book einen
kommaseparierten Autorentext, zum Beispiel:

   "Robert C. Martin"
   "Martin Fowler, Kent Beck"

Die Suche nach Autorennamen interpretiert diesen Text bewusst einfach:
Kommata trennen einzelne Autorinnen und Autoren. Innerhalb eines Autor-Tokens
gilt der letzte Namensbestandteil als Nachname. Eine Suche nach
AuthorLastName prüft deshalb nur diese extrahierten Nachnamen.

Diese Vereinfachung reduziert den Stoff im Catalog-Modul. Die Studierenden
sehen weiterhin, wie eine Suche über ein ReadModel funktioniert, müssen aber
keine zusätzliche Author-Entity, keine m:n-Zuordnung und keine Join-Tabelle
verstehen.

Die Anzahl der Exemplare wird ebenfalls hier berechnet:

- TotalBookItems: alle Exemplare eines Buches
- AvailableBookItems: nur verfügbare Exemplare

Damit wird sichtbar:

- Domain: schützt fachliche Regeln innerhalb des Aggregates.
- Repository: lädt Aggregate für Änderungen.
- ReadModel: projiziert Daten für Anzeige, Suche und Listen.
- UseCase: verändert Zustand.
- Controller: verwendet Schnittstellen, keine konkreten Klassen.

Die eigentliche fachlich wichtige m:n-Beziehung wird später im Loans-Modul
behandelt. Dort ist die Beziehung zwischen Reader und BookItem nicht nur eine
Struktur, sondern ein fachlicher Vorgang mit Ausleihdatum, Rückgabefrist,
Rückgabe, Verlängerung und Status.
*/