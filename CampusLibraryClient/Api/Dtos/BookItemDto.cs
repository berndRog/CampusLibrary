namespace CampusLibraryClient.Api.Dtos;

public sealed record BookItemDto(
   Guid Id,
   Guid BookId,
   string? InventoryNumber,
   int Status
);
