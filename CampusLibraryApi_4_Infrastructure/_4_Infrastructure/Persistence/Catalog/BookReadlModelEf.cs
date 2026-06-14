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
         .Include(b => b.Authors)
         .Include(b => b.BookItems)
         .AsSplitQuery()
         .SingleOrDefaultAsync(b => b.Id == id && b.IsActive, ct);

      if (book is null)
         return Result<BookDetailDto>.Failure(CatalogErrors.BookNotFound);

      return Result<BookDetailDto>.Success(ToBookDetailDto(book));
   }

   public async Task<Result<IReadOnlyList<BookListItemDto>>> SelectAllAsync(
      CancellationToken ct = default
   ) {
      var books = await dbContext.Books
         .AsNoTracking()
         .Include(b => b.Authors)
         .Include(b => b.BookItems)
         .AsSplitQuery()
         .Where(b => b.IsActive)
         .OrderBy(b => b.Title)
         .ToListAsync(ct);

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
         .Include(b => b.Authors)
         .Include(b => b.BookItems)
         .AsSplitQuery()
         .Where(b => b.IsActive);

      query = search.SearchField switch {
         BookSearchField.Title =>
            query.Where(b => EF.Functions.Like(b.Title, pattern)),

         BookSearchField.AuthorName =>
            query.Where(b =>
               b.Authors.Any(a =>
                  EF.Functions.Like(a.Firstname, pattern) ||
                  EF.Functions.Like(a.Lastname, pattern) ||
                  EF.Functions.Like(a.Firstname + " " + a.Lastname, pattern))),

         BookSearchField.Isbn =>
            query.Where(b => b.IsbnVo == IsbnVo.FromPersisted(normalizedSearchText)),

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

   public async Task<Result<IReadOnlyList<BookListItemDto>>> SelectByAuthorIdAsync(
      Guid authorId,
      CancellationToken ct = default
   ) {
      if (authorId == Guid.Empty)
         return Result<IReadOnlyList<BookListItemDto>>.Failure(CatalogErrors.InvalidAuthorId);

      var books = await dbContext.Books
         .AsNoTracking()
         .Include(b => b.Authors)
         .Include(b => b.BookItems)
         .AsSplitQuery()
         .Where(b => b.IsActive)
         .Where(b => b.Authors.Any(a => a.Id == authorId))
         .OrderBy(b => b.Title)
         .ToListAsync(ct);

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
         Authors: book.Authors
            .OrderBy(a => a.Lastname)
            .ThenBy(a => a.Firstname)
            .Select(a => a.DisplayName)
            .ToList(),
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
         Authors: book.Authors
            .OrderBy(a => a.Lastname)
            .ThenBy(a => a.Firstname)
            .Select(author => author.ToAuthorDto())
            .ToList(),
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
}

/*
Lernziele und Didaktik
----------------------

Dieses ReadModel gehört zur Query-Seite des Catalog-Moduls.

Es liefert keine Book-Aggregates an die Web-Schicht zurück, sondern DTOs, die
für Anzeige und Suche optimiert sind. Dadurch bleibt das Domain-Modell geschützt
und wird nicht direkt an Controller oder Clients weitergegeben.

FindByIdAsync liefert ein BookDetailDto für Detailansichten.
SelectAllAsync, SearchAsync und SelectByAuthorIdAsync liefern BookListItemDto
für Listen und Trefferanzeigen.

Die Autoren eines Buches werden im ReadModel sortiert, nicht in der Domain.
Das ist eine Anzeigeentscheidung: In der Liste sollen Autoren alphabetisch nach
Nachname und danach nach Vorname erscheinen.

Die Anzahl der Exemplare wird ebenfalls hier berechnet:

- TotalBookItems: alle Exemplare eines Buches
- AvailableBookItems: nur verfügbare Exemplare

Damit wird sichtbar:

- Domain: schützt fachliche Regeln innerhalb des Aggregates.
- Repository: lädt Aggregate für Änderungen.
- ReadModel: projiziert Daten für Anzeige, Suche und Listen.
- UseCase: verändert Zustand.
- Controller: verwendet Schnittstellen, keine konkreten Klassen.
*/