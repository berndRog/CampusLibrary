using CampusLibraryApi._2_BuildingBlocks._3_Domain.Entities;
namespace CampusLibraryApi._3_Core.Catalog._3_Domain.Entities;

public sealed class BookAuthor : Entity {
   
   public Guid BookId { get; private set; }
   public Guid AuthorId { get; private set; }
   public int SortOrder { get; private set; }

   private BookAuthor() {
      // Required by EF Core.
   }

   internal BookAuthor(
      Guid id,
      Guid bookId,
      Guid authorId,
      int sortOrder
   ) {
      BookId = bookId;
      AuthorId = authorId;
      SortOrder = sortOrder;
   }
}