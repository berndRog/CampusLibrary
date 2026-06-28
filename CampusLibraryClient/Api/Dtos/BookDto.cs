namespace CampusLibraryClient.Api.Dtos;

public sealed record BookDto(
   Guid Id,
   string? AuthorsText,
   string? Title,
   string? Subtitle,
   string? Isbn,
   int BookItemCount,
   bool IsActive
);
