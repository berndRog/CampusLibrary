namespace CampusLibraryClient.Api.Dtos;

public sealed record LoanCreateDto(
   Guid ReaderId,
   Guid BookItemId,
   string? Id = null
);
