using CampusLibraryApi._2_BuildingBlocks._3_Domain.Enums;
using CampusLibraryApi._2_BuildingBlocks._3_Domain.Errors;
namespace CampusLibraryApi._3_Core.Readers._3_Domain.Errors;

public static class ReaderErrors {

   public static readonly DomainError IdRequired =
      new(
         WebErrorStatus.BadRequest,
         "Reader: IdRequired",
         "Reader id is required."
      );

   public static readonly DomainError InvalidId =
      new(
         WebErrorStatus.BadRequest,
         "Reader: InvalidId",
         "The given Id is invalid."
      );


   public static readonly DomainError ReaderUpdateMeProfileDtoRequired =
      new(
         WebErrorStatus.BadRequest,
         "Reader: ReaderProfileUpdateDtoRequired",
         "A ReaderProfileUpdateDto object must be provided."
      );

   public static readonly DomainError FirstnameIsRequired =
      new(
         WebErrorStatus.BadRequest,
         "Reader: FirstnameRequired",
         "A first name must be provided."
      );

   public static readonly DomainError InvalidFirstname =
      new(
         WebErrorStatus.BadRequest,
         "Reader: InvalidFirstname",
         "The provided first name is too short or too long (2–80 characters)."
      );

   public static readonly DomainError LastnameIsRequired =
      new(
         WebErrorStatus.BadRequest,
         "Reader: LastnameRequired",
         "A last name must be provided."
      );

   public static readonly DomainError InvalidLastname =
      new(
         WebErrorStatus.BadRequest,
         "Reader: InvalidLastname",
         "The provided last name is too short or too long (2–80 characters)."
      );

   public static readonly DomainError InvalidEmail =
      new(
         WebErrorStatus.BadRequest,
         "Reader: InvalidEmail",
         "The email is invalid."
      );

   public static readonly DomainError EmailAlreadyInUse =
      new(
         WebErrorStatus.Conflict,
         "Reader: EmailAlreadyInUse",
         "The provided email address is already in use."
      );

   public static readonly DomainError AddressIsRequired =
      new(
         WebErrorStatus.BadRequest,
         "Reader: Address required",
         "A valid address with street, postal code and city must be provided."
      );

   public static readonly DomainError ReaderNotFound =
      new(
         WebErrorStatus.NotFound,
         "Reader: NotFound",
         "The reader was not found."
      );

   public static readonly DomainError SubjectAlreadyExists =
      new(
         WebErrorStatus.Conflict,
         "Reader: SubjectAlreadyExists",
         "A reader with this subject already exists."
      );

   public static readonly DomainError ReaderCreateDtoRequired =
      new(
         WebErrorStatus.BadRequest,
         "Reader: ReaderCreateDtoRequired",
         "A ReaderCreateDto object must be provided."
      );

   public static readonly DomainError ReaderUpdateMeDtoRequired =
      new(
         WebErrorStatus.BadRequest,
         "Reader: ReaderUpdateDtoRequired",
         "A ReaderUpdateDto object must be provided."
      );

   public static readonly DomainError IsAlreadyDeactivated =
      new(
         WebErrorStatus.BadRequest,
         "Reader: Is Already Deactivated",
         "The reader is deactivated."
      );

   public static readonly DomainError ReaderCannotBeDeactivatedWithLoans =
      new(
         WebErrorStatus.Conflict,
         "Reader: Cannot Be Deactivated With Loans",
         "The reader cannot be deactivated while current loans exist."
      );


   // Address value object
   // ------------------------------------------------------------------------
   public static readonly DomainError StreetIsRequired =
      new(
         WebErrorStatus.BadRequest,
         "Address: StreetRequired",
         "A street must be provided when specifying an address."
      );

   public static readonly DomainError InvalidStreet =
      new(
         WebErrorStatus.BadRequest,
         "Address: InvalidStreet",
         "The provided street name is too short or too long (2–80 characters)."
      );

   public static readonly DomainError PostalCodeIsRequired =
      new(
         WebErrorStatus.BadRequest,
         "Address: PostalCodeRequired",
         "A postal code must be provided when specifying an address."
      );

   public static readonly DomainError InvalidPostalCode =
      new(
         WebErrorStatus.BadRequest,
         "Address: InvalidPostalCode",
         "The provided postal code is too short or too long (2–10 characters)."
      );

   public static readonly DomainError CityIsRequired =
      new(
         WebErrorStatus.BadRequest,
         "Address: CityRequired",
         "A city must be provided when specifying an address."
      );

   public static readonly DomainError InvalidCity =
      new(
         WebErrorStatus.BadRequest,
         "Address: InvalidCity",
         "The provided city is too short or too long (2–80 characters)."
      );

   public static readonly DomainError InvalidCountry =
      new(
         WebErrorStatus.BadRequest,
         "Address: InvalidCountry",
         "The provided country is too short or too long (2–80 characters)."
      );

}
