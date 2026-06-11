using CampusLibraryApi._2_BuildingBlocks;
using CampusLibraryApi._2_BuildingBlocks._3_Domain.Entities;
using CampusLibraryApi._3_Core.Catalog._3_Domain.Errors;
using CampusLibraryApi._3_Core.Catalog._3_Domain.ValueObjects;
namespace CampusLibraryApi._3_Core.Catalog._3_Domain.Entities;

public sealed class Book : AggregateRoot {
   
   private readonly List<BookItem> _bookItems = [];
   private readonly List<BookAuthor> _bookAuthors = [];

   public string Title { get; private set; } = string.Empty;
   public string? Subtitle { get; private set; }
   public IsbnVo IsbnVo { get; private set; } = null!;

   public IReadOnlyCollection<BookItem> BookItems => _bookItems.AsReadOnly();
   public IReadOnlyCollection<BookAuthor> BookAuthors => _bookAuthors.AsReadOnly();

   // EF Core ctor
   private Book() {
   }
   // Domain ctor
   private Book(
      Guid id,
      string title,
      string? subtitle,
      IsbnVo isbnVo
   ) {
      Title = title;
      Subtitle = subtitle;
      IsbnVo = isbnVo;
   }

   public static Result<Book> Create(
      Guid id,
      string title,
      string? subtitle,
      string isbn
   ) {
      if (string.IsNullOrWhiteSpace(title))
         return Result<Book>.Failure(CatalogErrors.TitleIsRequired);

      var isbnResult = IsbnVo.Create(isbn);
      if (isbnResult.IsFailure)
         return Result<Book>.Failure(isbnResult.Error);

      var book = new Book(
         id,
         title.Trim(),
         string.IsNullOrWhiteSpace(subtitle) ? null : subtitle.Trim(),
         isbnResult.Value
      );

      return Result<Book>.Success(book);
   }

   public Result<BookItem> AddBookItem(
      Guid bookItemId,
      string inventoryNumber
   ) {
      if (_bookItems.Any(bi => bi.InventoryNumber == inventoryNumber))
         return Result<BookItem>.Failure(CatalogErrors.BookItemAlreadyExists);

      var bookItemResult = BookItem.Create(
         bookItemId,
         Id,
         inventoryNumber
      );

      if (bookItemResult.IsFailure)
         return Result<BookItem>.Failure(bookItemResult.Error);

      _bookItems.Add(bookItemResult.Value);

      return Result<BookItem>.Success(bookItemResult.Value);
   }

   public Result<BookAuthor> AssignAuthor(
      Guid bookAuthorId,
      Guid authorId,
      int sortOrder
   ) {
      if (_bookAuthors.Any(ba => ba.AuthorId == authorId))
         return Result<BookAuthor>.Failure(CatalogErrors.AuthorAlreadyAssigned);

      var bookAuthor = new BookAuthor(
         bookAuthorId,
         Id,
         authorId,
         sortOrder
      );

      _bookAuthors.Add(bookAuthor);

      return Result<BookAuthor>.Success(bookAuthor);
   }
}