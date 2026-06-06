namespace CampusLibrary.Api.Readers.Application.Dtos;

public sealed record ReaderCreateDto(
   string Subject,
   string Email,
   string DisplayName
);
