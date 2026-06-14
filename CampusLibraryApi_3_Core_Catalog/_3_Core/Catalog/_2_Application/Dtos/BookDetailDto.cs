namespace CampusLibraryApi._3_Core.Catalog._2_Application.Dtos;

// DTO for displaying the details of one Book.
// This DTO belongs to the query side and is optimized for read-only views.
public sealed record BookDetailDto(
   Guid Id,
   string Title,
   string? Subtitle,
   string Isbn,
   IReadOnlyList<AuthorDto> Authors,
   IReadOnlyList<BookItemDto> BookItems,
   int TotalBookItems,
   int AvailableBookItems,
   bool IsActive,
   DateTime CreatedAt,
   DateTime UpdatedAt
);
