namespace CampusLibraryClient.Api.Dtos;

public sealed record BookDetailDto(
   Guid Id,
   string? AuthorsText,
   string? Title,
   string? Subtitle,
   string? Isbn,
   IReadOnlyList<BookItemDto>? BookItems,
   int TotalItems,
   int AvailableItems,
   bool IsActive
);
