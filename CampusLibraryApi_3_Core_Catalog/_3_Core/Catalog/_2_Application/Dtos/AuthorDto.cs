namespace CampusLibraryApi._3_Core.Catalog._2_Application.Dtos;

public sealed record AuthorDto(
   Guid Id,
   string Firstname,
   string Lastname,
   string DisplayName
);