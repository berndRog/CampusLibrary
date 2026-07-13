namespace CampusLibraryClient.Api.Dtos;

public sealed record LoanDto(
   Guid Id,
   DateTime LoanDate,
   DateTime DueDate,
   Guid ReaderId,
   Guid BookItemId,
   int RenewalCount
);
