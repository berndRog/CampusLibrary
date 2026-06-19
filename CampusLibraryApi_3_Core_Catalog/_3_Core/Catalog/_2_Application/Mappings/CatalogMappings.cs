using CampusLibraryApi._3_Core.Catalog._2_Application.Dtos;
using CampusLibraryApi._3_Core.Catalog._3_Domain.Entities;
using CampusLibraryApi._3_Core.Catalog._3_Domain.Enums;

namespace CampusLibraryApi._3_Core.Catalog._2_Application.Mappings;

public static class CatalogMappings {
   
   public static BookDto ToBookDto(this Book book) =>
      new(
         Id: book.Id,
         AuthorsText: book.AuthorsText,
         Title: book.Title,
         Subtitle: book.Subtitle,
         Isbn: book.IsbnVo.Value,
         BookItemCount: book.BookItems.Count,
         IsActive: book.IsActive
      );
   
   public static BookItemDto ToBookItemDto(this BookItem bookItem) =>
      new(
         Id: bookItem.Id,
         BookId: bookItem.BookId,
         InventoryNumber: bookItem.InventoryNumber,
         Status: bookItem.Status
      );
}