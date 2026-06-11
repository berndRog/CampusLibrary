using CampusLibraryApi._2_BuildingBlocks;
using CampusLibraryApi._2_BuildingBlocks._3_Domain.Entities;
using CampusLibraryApi._3_Core.Catalog._3_Domain.Enums;
using CampusLibraryApi._3_Core.Catalog._3_Domain.Errors;
namespace CampusLibraryApi._3_Core.Catalog._3_Domain.Entities;

public sealed class BookItem : Entity {
   
   public Guid BookId { get; private set; }
   public string InventoryNumber { get; private set; } = string.Empty;
   public BookItemStatus Status { get; private set; }

   private BookItem() {
      // Required by EF Core.
   }

   internal BookItem(
      Guid id,
      Guid bookId,
      string inventoryNumber
   ) {
      BookId = bookId;
      InventoryNumber = inventoryNumber;
      Status = BookItemStatus.Available;
   }

   internal static Result<BookItem> Create(
      Guid id,
      Guid bookId,
      string inventoryNumber
   ) {
      if (string.IsNullOrWhiteSpace(inventoryNumber))
         return Result<BookItem>.Failure(CatalogErrors.BookItemInventoryNumberIsRequired);

      var bookItem = new BookItem(
         id,
         bookId,
         inventoryNumber.Trim()
      );

      return Result<BookItem>.Success(bookItem);
   }

   public void MarkAsUnavailable() =>
      Status = BookItemStatus.Unavailable;
   
   public void MarkAsAvailable() =>
      Status = BookItemStatus.Available;

   public void MarkAsLost() =>
      Status = BookItemStatus.Lost;

   public void MarkAsDamaged() =>
      Status = BookItemStatus.Damaged;
   
}