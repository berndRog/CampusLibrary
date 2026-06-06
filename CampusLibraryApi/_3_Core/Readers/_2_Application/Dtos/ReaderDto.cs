namespace CampusLibrary.Api._3_Core.Readers.Application.Dtos;

public sealed record ReaderDto(
   Guid Id,
   string Subject,
   string Email,
   string DisplayName
);
