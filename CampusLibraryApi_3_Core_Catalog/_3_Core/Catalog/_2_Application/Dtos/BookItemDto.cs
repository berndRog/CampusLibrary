using CampusLibraryApi._3_Core.Catalog._3_Domain.Enums;
namespace CampusLibraryApi._3_Core.Catalog._2_Application.Dtos;

public sealed record BookItemDto(
   Guid Id,
   Guid BookId,
   string InventoryNumber,
   BookItemStatus Status
);