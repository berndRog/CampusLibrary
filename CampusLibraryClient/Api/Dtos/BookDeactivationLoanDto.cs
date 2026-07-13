namespace CampusLibraryClient.Api.Dtos;

public sealed record BookDeactivationLoanDto(
   Guid BookItemId,
   string ReaderEmail,
   DateTime DueDate
);
