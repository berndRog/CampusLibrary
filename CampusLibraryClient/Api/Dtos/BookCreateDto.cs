namespace CampusLibraryClient.Api.Dtos;

public sealed record BookCreateDto(
   string? AuthorsText,
   string? Title,
   string? Subtitle,
   string? Isbn,
   string? Id = null
);
