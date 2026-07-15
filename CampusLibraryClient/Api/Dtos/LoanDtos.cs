namespace CampusLibraryClient.Api.Dtos;

public sealed record LoanDto(
   Guid Id,

   Guid ReaderId,
   string Firstname,
   string Lastname,
   string Email,

   Guid BookItemId,
   Guid BookId,
   string AuthorsText,
   string Title,
   string? Subtitle,
   string Isbn,
   bool BookIsActive,
   bool IsAvailableForLoan,

   DateTime LoanDate,
   DateTime DueDate,
   int RenewalCount,

   bool IsOverdue,
   bool CanRenew
);

public sealed record LoanCreateDto(
   Guid ReaderId,
   Guid BookItemId,
   string? Id = null
);

public sealed record LoanBorrowMeDto(
   Guid BookItemId,
   string? Id = null
);
