namespace CampusLibraryClient.Api.Dtos;

public sealed record BookItemDto(
   Guid Id,
   Guid BookId,
   int Status
);
