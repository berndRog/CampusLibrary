using CampusLibraryApi._3_Core.Catalog._2_Application.Dtos;
using CampusLibraryApi._3_Core.Catalog._3_Domain.Entities;
using CampusLibraryApi._3_Core.Catalog._3_Domain.Enums;

namespace CampusLibraryApi._3_Core.Catalog._2_Application.Mappings;

public static class CatalogMappings {

   public static AuthorDto ToAuthorDto(this Author author) =>
      new(
         Id: author.Id,
         Firstname: author.Firstname,
         Lastname: author.Lastname,
         DisplayName: author.DisplayName,
         IsActive: author.IsActive
      );

   public static BookDto ToBookDto(this Book bookAlt) =>
      new(
         Id: bookAlt.Id,
         Title: bookAlt.Title,
         Subtitle: bookAlt.Subtitle,
         Isbn: bookAlt.IsbnVo.Value,
         BookItemCount: bookAlt.BookItems.Count,
         IsActive: bookAlt.IsActive
      );

   public static BookItemDto ToBookItemDto(this BookItem bookItem) =>
      new(
         Id: bookItem.Id,
         BookId: bookItem.BookId,
         InventoryNumber: bookItem.InventoryNumber,
         Status: bookItem.Status
      );
}