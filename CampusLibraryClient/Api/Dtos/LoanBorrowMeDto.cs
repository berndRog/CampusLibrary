namespace CampusLibraryClient.Api.Dtos;

public sealed record LoanBorrowMeDto(
   Guid BookItemId,
   string? Id = null
);
