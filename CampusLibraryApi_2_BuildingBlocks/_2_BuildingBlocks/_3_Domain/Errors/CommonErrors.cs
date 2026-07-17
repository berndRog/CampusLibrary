using CampusLibraryApi._2_BuildingBlocks._3_Domain.Enums;
namespace CampusLibraryApi._2_BuildingBlocks._3_Domain.Errors;

public static class CommonErrors {

   public static readonly DomainError ReaderIdRequired =
      new(
         WebErrorStatus.BadRequest,
         "Contract: Reader Id Required",
         "The reader id is required."
      );

   public static readonly DomainError ReaderIsDeactivated =
      new(
         WebErrorStatus.BadRequest,
         "Contract: Reader Is Deactivated",
         "The reader is deactivated."
      );

   public static readonly DomainError BookItemIdRequired =
      new(
         WebErrorStatus.BadRequest,
         "Contract: Book Item Id Required",
         "The book item id is required."
      );

   public static readonly DomainError ReaderNotFound =
      new(
         WebErrorStatus.NotFound,
         "Contract: Reader Not Found",
         "The reader was not found."
      );

   public static readonly DomainError BookItemNotFound =
      new(
         WebErrorStatus.NotFound,
         "Contract: Book Item Not Found",
         "The book item was not found."
      );

}
