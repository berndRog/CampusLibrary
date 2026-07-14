using System.Linq.Expressions;
using CampusLibraryApi._3_Core.Catalog._2_Application.Dtos;
using CampusLibraryApi._3_Core.Catalog._2_Application.Enums;
using CampusLibraryApi._2_BuildingBlocks;
using CampusLibraryApi._2_BuildingBlocks._1_Ports;
using CampusLibraryApi._3_Core.Catalog._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Catalog._3_Domain.Entities;
using CampusLibraryApi._3_Core.Catalog._3_Domain.Enums;
using CampusLibraryApi._3_Core.Catalog._3_Domain.Errors;
using CampusLibraryApi._3_Core.Catalog._3_Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

using CampusLibraryApi._2_BuildingBlocks._1_Ports.Contracts;
namespace CampusLibraryApi._4_Infrastructure.Persistence.Catalog;

// EF Core query implementation for the public Book contract.
internal sealed class BookReadModelEf(
   ICatalogDbContext bookDbContext,
   ILoanCatalogContract loanCatalogContract
) : IBookReadModel {

   public async Task<Result<BookDto>> FindByIdAsync(
      Guid id,
      bool includeInactive = false,
      CancellationToken ct = default
   ) {
      BookDto? dto = await bookDbContext.Books
         .AsNoTracking()
         .Where(book => book.Id == id)
         .Where(book => includeInactive || book.IsActive)
         .Select(BookToDto)
         .FirstOrDefaultAsync(ct);

      return dto is null
         ? Result<BookDto>.Failure(CatalogErrors.BookNotFound)
         : Result<BookDto>.Success(dto);
   }

   public async Task<Result<BookDeactivationInfoDto>> FindDeactivationInfoAsync(
      Guid id,
      CancellationToken ct = default
   ) {
      if(id == Guid.Empty)
         return Result<BookDeactivationInfoDto>.Failure(CatalogErrors.InvalidBookId);

      bool bookExists = await bookDbContext.Books
         .AsNoTracking()
         .AnyAsync(book => book.Id == id, ct);

      if(!bookExists)
         return Result<BookDeactivationInfoDto>.Failure(CatalogErrors.BookNotFound);

      List<Guid> bookItemIds = await bookDbContext.BookItems
         .AsNoTracking()
         .Where(bookItem => bookItem.BookId == id)
         .Select(bookItem => bookItem.Id)
         .ToListAsync(ct);

      var loansResult = await loanCatalogContract.FindCurrentLoansForBookItemsAsync(
         bookItemIds: bookItemIds,
         ct: ct
      );

      if(loansResult.IsFailure)
         return Result<BookDeactivationInfoDto>.Failure(loansResult.Error);

      IReadOnlyList<BookLoanInfoDto> currentLoans = loansResult.Value
         .Select(loan => new BookLoanInfoDto(
            BookItemId: loan.BookItemId,
            ReaderEmail: loan.ReaderEmail,
            DueDate: loan.DueDate
         ))
         .ToList();

      return Result<BookDeactivationInfoDto>.Success(
         new BookDeactivationInfoDto(
            BookId: id,
            TotalItems: bookItemIds.Count,
            BorrowedItems: currentLoans.Count,
            CurrentLoans: currentLoans
         )
      );
   }

   public async Task<Result<IReadOnlyList<BookDto>>> SelectAllAsync(
      bool includeInactive = false,
      CancellationToken ct = default
   ) {
      List<BookDto> books = await bookDbContext.Books
         .AsNoTracking()
         .Where(book => includeInactive || book.IsActive)
         .OrderBy(book => book.Title)
         .ThenBy(book => book.Subtitle)
         .Select(BookToDto)
         .ToListAsync(ct);

      return Result<IReadOnlyList<BookDto>>.Success(books);
   }

   public async Task<Result<IReadOnlyList<BookDto>>> SearchAsync(
      BookSearchField searchField,
      string searchText,
      bool includeInactive = false,
      CancellationToken ct = default
   ) {
      searchText = searchText.Trim();

      if(string.IsNullOrWhiteSpace(searchText))
         return Result<IReadOnlyList<BookDto>>.Success([]);

      IQueryable<Book> query = bookDbContext.Books
         .AsNoTracking()
         .Where(book => includeInactive || book.IsActive);

      if(searchField == BookSearchField.Title) {
         string pattern = $"%{searchText}%";

         List<BookDto> books = await query
            .Where(book => EF.Functions.Like(book.Title, pattern))
            .OrderBy(book => book.Title)
            .ThenBy(book => book.Subtitle)
            .Select(BookToDto)
            .ToListAsync(ct);

         return Result<IReadOnlyList<BookDto>>.Success(books);
      }

      if(searchField == BookSearchField.Isbn) {
         string normalizedIsbn = searchText
            .Replace("-", string.Empty)
            .Replace(" ", string.Empty);

         var isbnVo = IsbnVo.FromPersisted(normalizedIsbn);

         List<BookDto> books = await query
            .Where(book => book.IsbnVo == isbnVo)
            .OrderBy(book => book.Title)
            .ThenBy(book => book.Subtitle)
            .Select(BookToDto)
            .ToListAsync(ct);

         return Result<IReadOnlyList<BookDto>>.Success(books);
      }

      if(searchField == BookSearchField.AuthorLastName) {
         List<Book> books = await query
            .Include(book => book.BookItems)
            .ToListAsync(ct);

         IReadOnlyList<BookDto> matches = books
            .Where(book => ContainsAuthorLastName(book.AuthorsText, searchText))
            .OrderBy(book => book.Title)
            .ThenBy(book => book.Subtitle)
            .Select(ToDto)
            .ToList();

         return Result<IReadOnlyList<BookDto>>.Success(matches);
      }

      return Result<IReadOnlyList<BookDto>>.Success([]);
   }

   private static readonly Expression<Func<Book, BookDto>> BookToDto =
      book => new BookDto(
         book.Id,
         book.AuthorsText,
         book.Title,
         book.Subtitle,
         book.IsbnVo.Value,
         book.BookItems
            .Select(item => new BookItemDto(
               item.Id,
               item.BookId,
               (int)item.Status
            ))
            .ToList(),
         book.BookItems.Count,
         book.BookItems.Count(item => item.Status == BookItemStatus.Available),
         book.IsActive
      );

   private static BookDto ToDto(Book book) => new(
      Id: book.Id,
      AuthorsText: book.AuthorsText,
      Title: book.Title,
      Subtitle: book.Subtitle,
      Isbn: book.IsbnVo.Value,
      BookItems: book.BookItems
         .Select(item => new BookItemDto(
            Id: item.Id,
            BookId: item.BookId,
            Status: (int)item.Status
         ))
         .ToList(),
      TotalItems: book.BookItems.Count,
      AvailableItems: book.BookItems.Count(
         item => item.Status == BookItemStatus.Available
      ),
      IsActive: book.IsActive
   );

   private static bool ContainsAuthorLastName(
      string authorsText,
      string searchText
   ) {
      string normalizedSearchText = searchText.Trim();
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

   private static string ExtractLastName(string authorText) {
      string[] parts = authorText
         .Trim()
         .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

      return parts.Length == 0 ? string.Empty : parts[^1];
   }
}
