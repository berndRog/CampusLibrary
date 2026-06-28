namespace CampusLibraryClient.Api.Dtos;

public sealed record LoanListItemDto(
   Guid Id,
   Guid ReaderId,
   string? Firstname,
   string? Lastname,
   Guid BookItemId,
   string? InventoryNumber,
   string? Title,
   string? Subtitle,
   DateTime LoanDate,
   DateTime DueDate,
   int Status,
   bool IsOverdue
);
