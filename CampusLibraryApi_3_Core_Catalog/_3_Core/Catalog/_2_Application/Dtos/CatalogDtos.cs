namespace CampusLibraryApi._3_Core.Catalog._2_Application.Dtos;


// Response Dto
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

// Response Dto
public sealed record BookItemDto(
   Guid Id,
   Guid BookId,
   int Status
);

// Request Dto
public sealed record BookCreateDto(
   string AuthorsText,
   string Title,
   string? Subtitle,
   string Isbn,
   string? Id = null
);

// Request Dto
public sealed record BookItemAddDto(
   string? Id = null
);

// Response Dto ReadModel
public sealed record BookDeactivationInfoDto(
   Guid BookId,
   int TotalItems,
   int BorrowedItems,
   IReadOnlyList<BookLoanInfoDto> CurrentLoans
);

// Response Dto ReadModel
public sealed record BookLoanInfoDto(
   Guid BookItemId,
   string ReaderEmail,
   DateTime DueDate
);
