namespace CampusLibraryClient.Api.Dtos;

public sealed record ReaderDto(
   Guid Id,
   string? Firstname,
   string? Lastname,
   string? Email,
   AddressDto? AddressDto,
   bool IsActive,
   string? Subject,
   bool IsProfileCompleted
);
