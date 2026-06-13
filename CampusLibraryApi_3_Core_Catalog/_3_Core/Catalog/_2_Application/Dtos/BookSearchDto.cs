using CampusLibraryApi._3_Core.Catalog._2_Application.Enums;
namespace CampusLibraryApi._3_Core.Catalog._2_Application.Dtos;

public sealed record BookSearchDto(
   BookSearchField SearchField,
   string SearchText
);