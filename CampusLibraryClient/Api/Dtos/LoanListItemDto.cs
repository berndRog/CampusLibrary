namespace CampusLibraryClient.Api.Dtos;

public sealed record LoanListItemDto(
   Guid Id,
   Guid ReaderId,
   string? Firstname,
   string? Lastname,
   Guid BookItemId,
   string? Title,
   string? Subtitle,
   DateTime LoanDate,
   DateTime DueDate,
   bool IsOverdue
);
