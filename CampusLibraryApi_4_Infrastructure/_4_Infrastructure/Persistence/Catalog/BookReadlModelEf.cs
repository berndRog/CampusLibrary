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