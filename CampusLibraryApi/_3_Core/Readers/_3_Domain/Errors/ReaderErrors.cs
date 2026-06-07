using System.Runtime.InteropServices.JavaScript;
using CampusLibraryApi._2_Shared._3_Domain.Enums;
using CampusLibraryApi._2_Shared._3_Domain.Errors;
namespace CampusLibraryApi._3_Core.Readers._3_Domain.Errors;

public static class ReaderErrors {
   
   public static readonly DomainError IdRequired =
      new(WebErrorStatus.BadRequest,
         "Reader: Id is Required", 
         "Reader id is required.");

   public static readonly DomainError InvalidId =
      new(WebErrorStatus.BadRequest,
         "Reader: Invalid Id",
         "The given Id is invalid.");
   
   public static readonly DomainError SubjectRequired =
      new(WebErrorStatus.BadRequest,
         "Reader.SubjectRequired", 
         "Subject is required.");

   public static readonly DomainError FirstnameIsRequired =
      new(WebErrorStatus.BadRequest,
         "Reader: First name required",
         "A first name must be provided.");

   public static readonly DomainError InvalidFirstname =
      new(WebErrorStatus.BadRequest,
         "Reader: Invalid first name", 
         "The provided first name is too short or too long (2–80 characters).");

   public static readonly DomainError LastnameIsRequired =
      new(WebErrorStatus.BadRequest,
         "Reader: Last name required",
         "A last name must be provided.");

   public static readonly DomainError InvalidLastname =
      new(WebErrorStatus.BadRequest,
         "Reader: Invalid last name",
         "The provided last name is too short or too long (2–80 characters).");

   public static readonly DomainError InvalidEmail =
      new(WebErrorStatus.BadRequest,
         "Reader: Email is invalid", 
         "The email is invalid.");

   public static readonly DomainError EmailAlreadyInUse =
      new(WebErrorStatus.BadRequest,
         "Email: Email already used",
         "The provided email address is already in use.");
   
   public static readonly DomainError ReaderNotFound =
      new(WebErrorStatus.NotFound,
         "Reader: Not Found", 
         "The reader was not found.");

   public static readonly DomainError SubjectAlreadyExists =
      new(WebErrorStatus.Conflict,
         "Reader: Subject already exists",
         "A reader with this subject already exists.");
   
   public static readonly DomainError CustomerCreateDtoRequired =
      new(WebErrorStatus.BadRequest,
         "Reader: ReaderCreateDto required",
         "A ReaderCreateDto object must must be provided.");

   
   // Address (Value Object)
   // ------------------------------------------------------------------------
   public static readonly DomainError StreetIsRequired =
      new(WebErrorStatus.BadRequest,
         "Address: Street is required",
         "A street must be provided when specifying an address.");
   
   public static readonly DomainError InvalidStreet =
      new(WebErrorStatus.BadRequest,
         "Address: Invalid street name",
         "The provided street name is too short or too long (2–80 characters).");

   public static readonly DomainError PostalCodeIsRequired =
      new(WebErrorStatus.BadRequest,
         "Address: Postal code is required",
         "A postal code must be provided when specifying an address."
      );
   public static readonly DomainError InvalidPostalCode =
      new(WebErrorStatus.BadRequest,
         "Address: Invalid postal code",
         "The provided postal code is too short or too long (2–10 characters).");

   public static readonly DomainError CityIsRequired =
      new(
         WebErrorStatus.BadRequest,
         "Address:City is required",
         "A city must be provided when specifying an address."
      );
   public static readonly DomainError InvalidCity =
      new(WebErrorStatus.BadRequest,
         "Address: Invalid city",
         "The provided city is too short or too long (2–80 characters).");

   public static readonly DomainError InvalidCountry =
      new(WebErrorStatus.BadRequest,
         "Address: Invalid country",
         "The provided country is too short or too long (2–80 characters).");
}