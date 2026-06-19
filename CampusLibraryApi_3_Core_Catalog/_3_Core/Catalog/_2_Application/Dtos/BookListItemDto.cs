namespace CampusLibraryApi._3_Core.Catalog._2_Application.Dtos;

public sealed record BookListItemDto(
   Guid Id,
   string AuthorsText,
   string Title,
   string? Subtitle,
   string Isbn,
   int TotalBookItems,
   int AvailableBookItems
);