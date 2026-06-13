namespace CampusLibraryApi._3_Core.Catalog._2_Application.Dtos;

public sealed record BookAssignAuthorDto(
   Guid BookId,
   Guid AuthorId,
   string? Id
);