namespace CampusLibraryClient.Api.Dtos;

public enum BookSearchField {
   Title = 1,
   AuthorLastName = 2,
   Isbn = 3
}

public sealed record BookDto(
   Guid Id,
   string AuthorsText,
   string Title,
   string? Subtitle,
   string Isbn,
   IReadOnlyList<BookItemDto> BookItems,
   int TotalItems,
   int AvailableItems,
   bool IsActive
);

public sealed record BookItemDto(
   Guid Id,
   Guid BookId,
   int Status
);

public sealed record BookCreateDto(
   string AuthorsText,
   string Title,
   string? Subtitle,
   string Isbn,
   string? Id = null
);

public sealed record BookItemAddDto(
   string? Id = null
);

public sealed record BookDeactivationInfoDto(
   Guid BookId,
   int TotalItems,
   int BorrowedItems,
   IReadOnlyList<BookLoanInfoDto> CurrentLoans
);

public sealed record BookLoanInfoDto(
   Guid BookItemId,
   string ReaderEmail,
   DateTime DueDate
);
