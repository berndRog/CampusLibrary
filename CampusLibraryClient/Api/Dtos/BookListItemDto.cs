namespace CampusLibraryClient.Api.Dtos;

public sealed record BookListItemDto(
   Guid Id,
   string? AuthorsText,
   string? Title,
   string? Subtitle,
   string? Isbn,
   int TotalItems,
   int AvailableItems,
   bool IsActive
);
