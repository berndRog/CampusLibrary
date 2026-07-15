using CampusLibraryApi._2_BuildingBlocks._3_Domain.Enums;
using CampusLibraryApi._2_BuildingBlocks._3_Domain.Errors;
namespace CampusLibraryApi._3_Core.Loans._3_Domain.Errors;

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
   
   public static readonly DomainError SubjectRequired =
      new(
         WebErrorStatus.BadRequest,
         "Reader: SubjectRequired",
         "Subject is required."
      );
   
   public static readonly DomainError InvalidIdentitySubject =
      new(
         WebErrorStatus.BadRequest,
         "Reader: Invalid IdentitySubject",
         "The provided sub is not valid."
      );
   
   public static readonly DomainError IdentityUnauthenticated =
      new(
         WebErrorStatus.Unauthorized,
         "Reader: IdentityUnauthenticated",
         "The current request is not authenticated."
      );
   
   public static readonly DomainError AccessNotAllowed =
      new(
         WebErrorStatus.Forbidden,
         "Reader: AccessNotAllowed",
         "The current user is not allowed to perform this reader operation."
      );

   public static readonly DomainError NotProvisioned =
      new(
         WebErrorStatus.NotFound,
         Title: "Reader: Is not provisioned",
         Message: "No reader with the given sub exists."
      );

   
   public static readonly DomainError IdentityEmailRequired =
      new(
         WebErrorStatus.BadRequest,
         "Reader: IdentityEmailRequired",
         "The authenticated user must provide an email claim."
      );
   
   public static readonly DomainError TimestampInvalid =
      new(
         WebErrorStatus.BadRequest,
         "Reader: Timestamp is invalid",
         "The timestamp from IA-Server is invalid."
      );
   

   
}
