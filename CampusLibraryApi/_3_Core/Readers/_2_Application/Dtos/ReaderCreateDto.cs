namespace CampusLibrary.Api._3_Core.Readers.Application.Dtos;

public sealed record ReaderCreateDto(
   string Subject,
   string Email,
   string DisplayName
);
