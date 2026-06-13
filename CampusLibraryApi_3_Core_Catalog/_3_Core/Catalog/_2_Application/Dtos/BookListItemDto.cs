namespace CampusLibraryApi._3_Core.Catalog._2_Application.Dtos;

public sealed record BookListItemDto(
   Guid Id,
   string Title,
   string? Subtitle,
   string Isbn,
   IReadOnlyList<string> Authors,
   int TotalBookItems,
   int AvailableBookItems
);