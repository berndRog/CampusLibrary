using CampusLibraryApi._2_BuildingBlocks._3_Domain.Enums;
using CampusLibraryApi._2_BuildingBlocks._3_Domain.Errors;

namespace CampusLibraryApi._3_Core.Catalog._3_Domain.Errors;

public static class CatalogErrors {

   // Book aggregate
   // ------------------------------------------------------------------------
   public static readonly DomainError BookIdRequired =
      new(
         WebErrorStatus.BadRequest,
         "Catalog: Book Id Required",
         "The book id is required."
      );

   public static readonly DomainError InvalidBookId =
      new(
         WebErrorStatus.BadRequest,
         "Catalog: Invalid Book Id",
         "The given Book Id is invalid."
      );

   public static readonly DomainError BookNotFound =
      new(
         WebErrorStatus.NotFound,
         "Catalog: Book Not Found",
         "The book was not found."
      );

   public static readonly DomainError AuthorsAreRequired =
      new(
         WebErrorStatus.BadRequest,
         "Catalog: Book Author(s) Is/Are Required",
         "At least one author is required."
   );

   public static readonly DomainError TitleIsRequired =
      new(
         WebErrorStatus.BadRequest,
         "Catalog: Book Title Required",
         "A book title must be provided."
      );

   public static readonly DomainError InvalidTitle =
      new(
         WebErrorStatus.BadRequest,
         "Catalog: Book Invalid Title",
         "The provided book title is too short or too long (2–200 characters)."
      );

   public static readonly DomainError InvalidSubtitle =
      new(
         WebErrorStatus.BadRequest,
         "Catalog: Book Invalid Subtitle",
         "The provided subtitle is too long (maximum 200 characters)."
      );

   public static readonly DomainError BookAlreadyExists =
      new(
         WebErrorStatus.Conflict,
         "Catalog: Book Already Exists",
         "A book with this ISBN already exists."
      );

   public static readonly DomainError BookCannotBeDeactivatedWithLoans =
      new(
         WebErrorStatus.Conflict,
         "Catalog: Book Cannot Be Deactivated With Loans",
         "The book cannot be deactivated while one of its book items is borrowed."
      );

   public static readonly DomainError BookCreateDtoRequired =
      new(
         WebErrorStatus.BadRequest,
         "Book: BookCreateDtoRequired",
         "A BookCreateDto object must be provided."
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
   public static readonly DomainError InvalidAuthorId =
      new(
         WebErrorStatus.BadRequest,
         "Catalog: Invalid uthor Id",
         "The given author Id is invalid."
      );

   public static readonly DomainError AuthorIdRequired =
      new(
         WebErrorStatus.BadRequest,
         "Catalog: Author Id Required",
         "The author id is required."
      );

   public static readonly DomainError AuthorNotFound =
      new(
         WebErrorStatus.NotFound,
         "Catalog: Author NotFound",
         "The author was not found."
      );

   public static readonly DomainError FirstnameIsRequired =
      new(
         WebErrorStatus.BadRequest,
         "Catalog: Author Firstname Required",
         "A first name must be provided."
      );

   public static readonly DomainError InvalidFirstname =
      new(
         WebErrorStatus.BadRequest,
         "Catalog: Author Invalid Firstname",
         "The provided first name is too short or too long (2–80 characters)."
      );

   public static readonly DomainError LastnameIsRequired =
      new(
         WebErrorStatus.BadRequest,
         "Catalog: Author Lastname Required",
         "A last name must be provided."
      );

   public static readonly DomainError InvalidLastname =
      new(
         WebErrorStatus.BadRequest,
         "Catalog: Author Invalid Lastname",
         "The provided last name is too short or too long (2–80 characters)."
      );

   public static readonly DomainError AuthorAlreadyExists =
      new(
         WebErrorStatus.Conflict,
         "Catalog: Author Already Exists",
         "An author with this name already exists."
      );


   public static readonly DomainError AuthorCreateDtoRequired =
      new(
         WebErrorStatus.BadRequest,
         "Author: AuthorCreateDtoRequired",
         "An AuthorCreateDto object must be provided."
      );


   // BookAuthor join entity
   // ------------------------------------------------------------------------
   public static readonly DomainError BookAuthorIdRequired =
      new(
         WebErrorStatus.BadRequest,
         "Catalog: Book Author Id Required",
         "The book author id is required."
      );

   public static readonly DomainError InvalidBookAuthorId =
      new(
         WebErrorStatus.BadRequest,
         "Catalog: Invalid Book Author Id",
         "The given book author Id is invalid."
      );

   public static readonly DomainError AuthorAlreadyAssigned =
      new(
         WebErrorStatus.Conflict,
         "Catalog: Book Author Already Assigned",
         "The author is already assigned to this book."
      );

   public static readonly DomainError BookAuthorNotFound =
      new(
         WebErrorStatus.NotFound,
         "Catalog: Book Author NotFound",
         "The book-author assignment was not found."
      );


   // BookItem entity
   // ------------------------------------------------------------------------
   public static readonly DomainError BookItemIdRequired =
      new(
         WebErrorStatus.BadRequest,
         "Catalog: Book Item Id Required",
         "The book item id is required."
      );

   public static readonly DomainError InvalidBookItemId =
      new(
         WebErrorStatus.BadRequest,
         "Catalog: Invalid BookItem Id",
         "The given BookItem Id is invalid."
      );

   public static readonly DomainError BookItemNotFound =
      new(
         WebErrorStatus.NotFound,
         "BookItem: NotFound",
         "The book item was not found."
      );

   public static readonly DomainError InvalidBookItemStatus =
      new(
         WebErrorStatus.BadRequest,
         "BookItem: InvalidStatus",
         "The given book item status is invalid."
      );

   public static readonly DomainError BookItemAddDtoRequired =
      new(
         WebErrorStatus.BadRequest,
         "Book: BookAddItemDtoRequired",
         "A BookAddItemDto object must be provided."
      );

   public static readonly DomainError BookAssignAuthorDtoRequired =
      new(
         WebErrorStatus.BadRequest,
         "Book: BookAssignAuthorDtoRequired",
         "A BookAssignAuthorDto object must be provided."
      );
}