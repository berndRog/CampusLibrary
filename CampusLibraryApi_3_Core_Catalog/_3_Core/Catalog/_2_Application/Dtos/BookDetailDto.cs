namespace CampusLibraryApi._3_Core.Catalog._2_Application.Dtos;

// API Response Dto
public sealed record BookDetailDto(
   Guid Id,
   string AuthorsText,
   string Title,
   string? Subtitle,
   string Isbn,
   IReadOnlyList<BookItemDto> BookItems,
   int TotalItems,
   int AvailableItems,
   bool IsActive
);
