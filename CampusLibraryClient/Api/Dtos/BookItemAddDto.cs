namespace CampusLibraryClient.Api.Dtos;

public sealed record BookItemAddDto(
   string? InventoryNumber,
   string? Id = null
);
