namespace CampusLibraryApi._3_Core.Catalog._2_Application.Dtos;

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

// Id remains optional to support deterministic tests.
public sealed record BookCreateDto(
   string AuthorsText,
   string Title,
   string? Subtitle,
   string Isbn,
   string? Id = null
);

// The request contains only the optional deterministic test id.
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
