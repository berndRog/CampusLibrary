using CampusLibraryApi._2_Shared._3_Domain.Errors;
namespace CampusLibraryApi._3_Core.Readers._3_Domain.Errors;

public static class ReaderErrors {

   public static readonly Error IdRequired =
      new("Reader.IdRequired", "Reader id is required.");

   public static readonly Error SubjectRequired =
      new("Reader.SubjectRequired", "Subject is required.");

   public static readonly Error EmailRequired =
      new("Reader.EmailRequired", "Email is required.");

   public static readonly Error EmailInvalid =
      new("Reader.EmailInvalid", "Email is invalid.");

   public static readonly Error EmailTooLong =
      new("Reader.EmailTooLong", "Email is too long.");

   public static readonly Error DisplayNameRequired =
      new("Reader.DisplayNameRequired", "Display name is required.");

   public static readonly Error ReaderNotFound =
      new("Reader.NotFound", "Reader was not found.");

   public static readonly Error SubjectAlreadyExists =
      new("Reader.SubjectAlreadyExists", "A reader with this subject already exists.");
}
