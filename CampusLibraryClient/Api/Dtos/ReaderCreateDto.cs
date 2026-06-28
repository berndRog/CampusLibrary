namespace CampusLibraryClient.Api.Dtos;

public sealed record ReaderCreateDto(
   string? Firstname,
   string? Lastname,
   string? Email,
   AddressDto? AddressDto,
   string? Subject,
   string? Id = null
);
