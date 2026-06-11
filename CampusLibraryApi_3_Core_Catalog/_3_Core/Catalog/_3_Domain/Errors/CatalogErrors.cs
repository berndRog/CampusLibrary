using CampusLibraryApi._2_BuildingBlocks._3_Domain.Enums;
using CampusLibraryApi._2_BuildingBlocks._3_Domain.Errors;

namespace CampusLibraryApi._3_Core.Catalog._3_Domain.Errors;

public static class CatalogErrors {

   // Book aggregate
   // ------------------------------------------------------------------------
   public static readonly DomainError IdRequired =
      new(
         WebErrorStatus.BadRequest,
         "Catalog: IdRequired",
         "Catalog id is required."
      );

   public static readonly DomainError InvalidId =
      new(
         WebErrorStatus.BadRequest,
         "Catalog: InvalidId",
         "The given Id is invalid."
      );

   public static readonly DomainError BookNotFound =
      new(
         WebErrorStatus.NotFound,
         "Book: NotFound",
         "The book was not found."
      );

   public static readonly DomainError TitleIsRequired =
      new(
         WebErrorStatus.BadRequest,
         "Book: TitleRequired",
         "A book title must be provided."
      );

   public static readonly DomainError InvalidTitle =
      new(
         WebErrorStatus.BadRequest,
         "Book: InvalidTitle",
         "The provided book title is too short or too long (2–200 characters)."
      );

   public static readonly DomainError InvalidSubtitle =
      new(
         WebErrorStatus.BadRequest,
         "Book: InvalidSubtitle",
         "The provided subtitle is too long (maximum 200 characters)."
      );

   public static readonly DomainError BookAlreadyExists =
      new(
         WebErrorStatus.Conflict,
         "Book: AlreadyExists",
         "A book with this ISBN already exists."
      );

   // ISBN value object
   // ------------------------------------------------------------------------
   public static readonly DomainError IsbnIsRequired =
      new(
         WebErrorStatus.BadRequest,
         "ISBN: Required",
         "An ISBN must be provided."
      );

   public static readonly DomainError IsbnMustHave13Digits =
      new(
         WebErrorStatus.BadRequest,
         "ISBN: MustHave13Digits",
         "The ISBN must have exactly 13 digits."
      );

   public static readonly DomainError IsbnMustContainOnlyDigits =
      new(
         WebErrorStatus.BadRequest,
         "ISBN: MustContainOnlyDigits",
         "The ISBN must contain only digits."
      );

   public static readonly DomainError IsbnChecksumInvalid =
      new(
         WebErrorStatus.BadRequest,
         "ISBN: ChecksumInvalid",
         "The ISBN checksum is invalid."
      );

   // Author aggregate
   // ------------------------------------------------------------------------
   public static readonly DomainError AuthorNotFound =
      new(
         WebErrorStatus.NotFound,
         "Author: NotFound",
         "The author was not found."
      );

   public static readonly DomainError AuthorNameIsRequired =
      new(
         WebErrorStatus.BadRequest,
         "Author: NameRequired",
         "An author name must be provided."
      );

   public static readonly DomainError FirstnameIsRequired =
      new(
         WebErrorStatus.BadRequest,
         "Author: FirstnameRequired",
         "A first name must be provided."
      );

   public static readonly DomainError InvalidFirstname =
      new(
         WebErrorStatus.BadRequest,
         "Author: InvalidFirstname",
         "The provided first name is too short or too long (2–80 characters)."
      );

   public static readonly DomainError LastnameIsRequired =
      new(
         WebErrorStatus.BadRequest,
         "Author: LastnameRequired",
         "A last name must be provided."
      );

   public static readonly DomainError InvalidLastname =
      new(
         WebErrorStatus.BadRequest,
         "Author: InvalidLastname",
         "The provided last name is too short or too long (2–80 characters)."
      );

   public static readonly DomainError AuthorAlreadyExists =
      new(
         WebErrorStatus.Conflict,
         "Author: AlreadyExists",
         "An author with this name already exists."
      );

   // BookAuthor join entity
   // ------------------------------------------------------------------------
   public static readonly DomainError AuthorAlreadyAssigned =
      new(
         WebErrorStatus.Conflict,
         "BookAuthor: AuthorAlreadyAssigned",
         "The author is already assigned to this book."
      );

   public static readonly DomainError BookAuthorNotFound =
      new(
         WebErrorStatus.NotFound,
         "BookAuthor: NotFound",
         "The book-author assignment was not found."
      );

   public static readonly DomainError InvalidSortOrder =
      new(
         WebErrorStatus.BadRequest,
         "BookAuthor: InvalidSortOrder",
         "The author sort order must be greater than or equal to zero."
      );

   // BookItem entity
   // ------------------------------------------------------------------------
   public static readonly DomainError BookItemNotFound =
      new(
         WebErrorStatus.NotFound,
         "BookItem: NotFound",
         "The book item was not found."
      );

   public static readonly DomainError BookItemInventoryNumberIsRequired =
      new(
         WebErrorStatus.BadRequest,
         "BookItem: InventoryNumberRequired",
         "An inventory number must be provided."
      );

   public static readonly DomainError InvalidInventoryNumber =
      new(
         WebErrorStatus.BadRequest,
         "BookItem: InvalidInventoryNumber",
         "The inventory number is too short or too long (2–40 characters)."
      );

   public static readonly DomainError BookItemAlreadyExists =
      new(
         WebErrorStatus.Conflict,
         "BookItem: AlreadyExists",
         "A book item with this inventory number already exists."
      );

   public static readonly DomainError InvalidBookItemStatus =
      new(
         WebErrorStatus.BadRequest,
         "BookItem: InvalidStatus",
         "The given book item status is invalid."
      );
}