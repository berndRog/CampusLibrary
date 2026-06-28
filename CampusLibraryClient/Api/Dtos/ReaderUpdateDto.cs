namespace CampusLibraryClient.Api.Dtos;

public sealed record ReaderUpdateDto(
   string? Lastname,
   string? Email,
   AddressDto? AddressDto
);
