using CampusLibraryApi._3_Core.Catalog._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Catalog._2_Application.Dtos;
using CampusLibraryApi._3_Core.Catalog._2_Application.Enums;
using CampusLibraryApi._3_Core.Catalog._3_Domain.Entities;
using CampusLibraryApi._3_Core.Catalog._3_Domain.Enums;
using CampusLibraryApi._3_Core.Catalog._3_Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace CampusLibraryApi._4_Infrastructure.Persistence.Catalog;

internal sealed class BookReadModelEf(
   ICatalogDbContext dbContext
) : IBookReadModel {

   public async Task<IReadOnlyList<BookListItemDto>> SearchAsync(
      BookSearchDto search,
      CancellationToken ct = default
   ) {
      if (string.IsNullOrWhiteSpace(search.SearchText))
         return [];

      var searchText = search.SearchText.Trim();

      var query = dbContext.Books
         .AsNoTracking()
         .Include(b => b.Authors)
         .Include(b => b.BookItems)
         .AsSplitQuery()
         .AsQueryable();

      query = search.SearchField switch {
         BookSearchField.Title =>
            query.Where(b =>
               b.Title.Contains(searchText)),

         BookSearchField.AuthorName =>
            query.Where(b =>
               b.Authors.Any(a =>
                  a.Firstname.Contains(searchText) ||
                  a.Lastname.Contains(searchText) ||
                  (a.Firstname + " " + a.Lastname).Contains(searchText))),

         BookSearchField.Isbn =>
            ApplyIsbnSearch(query, searchText),

         _ =>
            query.Where(_ => false)
      };

      var books = await query
         .OrderBy(b => b.Title)
         .ToListAsync(ct);

      return books
         .Select(ToBookListItemDto)
         .ToList();
   }

   public async Task<IReadOnlyList<BookListItemDto>> SelectByAuthorIdAsync(
      Guid authorId,
      CancellationToken ct = default
   ) {
      if (authorId == Guid.Empty)
         return [];

      var books = await dbContext.Books
         .AsNoTracking()
         .Include(b => b.Authors)
         .Include(b => b.BookItems)
         .AsSplitQuery()
         .Where(b => b.Authors.Any(a => a.Id == authorId))
         .OrderBy(b => b.Title)
         .ToListAsync(ct);

      return books
         .Select(ToBookListItemDto)
         .ToList();
   }

   private static IQueryable<Book> ApplyIsbnSearch(
      IQueryable<Book> query,
      string searchText
   ) {
      var resultIsbn = IsbnVo.Create(searchText);

      if (resultIsbn.IsFailure)
         return query.Where(_ => false);

      var isbnVo = resultIsbn.Value;

      return query.Where(b => b.IsbnVo == isbnVo);
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
}