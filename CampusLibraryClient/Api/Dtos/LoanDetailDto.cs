namespace CampusLibraryClient.Api.Dtos;

public sealed record LoanDetailDto(
   Guid Id,
   Guid ReaderId,
   string? Firstname,
   string? Lastname,
   Guid BookItemId,
   string? InventoryNumber,
   Guid BookId,
   string? AuthorsText,
   string? Title,
   string? Subtitle,
   string? Isbn,
   bool BookIsActive,
   bool IsAvailableForLoan,
   DateTime LoanDate,
   DateTime DueDate,
   DateTime? ReturnedAt,
   int Status,
   int RenewalCount,
   bool IsOverdue,
   bool CanRenew
);
