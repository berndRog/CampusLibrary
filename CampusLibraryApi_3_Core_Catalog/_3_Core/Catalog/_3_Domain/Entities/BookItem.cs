using CampusLibraryApi._2_BuildingBlocks;
using CampusLibraryApi._2_BuildingBlocks._3_Domain.Entities;
using CampusLibraryApi._3_Core.Catalog._3_Domain.Enums;
using CampusLibraryApi._3_Core.Catalog._3_Domain.Errors;
namespace CampusLibraryApi._3_Core.Catalog._3_Domain.Entities;

public sealed class BookItem : Entity {
   
   //--- properties ------------------------------------------------------------
   // Inherited from Entity / AggregateRoot:
   // public Guid Id { get; protected set; }
   public Guid BookId { get; private set; }
   public BookItemStatus Status { get; private set; }
   
   //--- constructors ----------------------------------------------------------
   // Required by EF Core.
   private BookItem() {
   }
   // Domain ctor
   internal BookItem(
      Guid id,
      Guid bookId
   ) {
      Id = id;
      BookId = bookId;
      Status = BookItemStatus.Available;
   }

   //--- factory methods -------------------------------------------------------
   // Creates a new BookItem object
   // Validation errors are returned as Result failures.
   internal static Result<BookItem> Create(
      Guid id,
      Guid bookId
   ) {
      
      if (id == Guid.Empty)
         return Result<BookItem>.Failure(CatalogErrors.BookItemIdRequired);
      if(bookId == Guid.Empty)
         return Result<BookItem>.Failure(CatalogErrors.BookIdRequired);
     
      var bookItem = new BookItem(
         id,
         bookId
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