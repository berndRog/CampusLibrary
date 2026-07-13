using AwesomeAssertions;
using CampusLibraryApi._3_Core.Catalog._3_Domain.Entities;
using CampusLibraryApi._3_Core.Catalog._3_Domain.Enums;
using CampusLibraryApi._3_Core.Catalog._3_Domain.Errors;
using CampusLibraryApiTest.TestInfrastructure;

namespace CampusLibraryApiTest._3_Core.Catalog._3_Domain;

public sealed class BookUt {
   private readonly TestSeed _seed = new TestSeed();

   [Fact]
   public void Create_WithValidBook_ShouldReturnSuccess() {
      // Arrange
      var book1 = _seed.Book1();

      // Act
      var result = Book.Create(
         id: book1.Id,
         authorsText: book1.AuthorsText,
         title: book1.Title,
         subtitle: book1.Subtitle,
         isbn: book1.IsbnVo.Value,
         createdAt: book1.CreatedAt
      );

      // Assert
      result.IsSuccess.Should().BeTrue();

      var actualBook1 = result.Value;

      actualBook1.Id.Should().Be(book1.Id);
      actualBook1.AuthorsText.Should().Be(book1.AuthorsText);
      actualBook1.Title.Should().Be(book1.Title);
      actualBook1.Subtitle.Should().Be(book1.Subtitle);
      actualBook1.IsbnVo.Should().Be(book1.IsbnVo);
      actualBook1.CreatedAt.Should().Be(book1.CreatedAt);
      actualBook1.UpdatedAt.Should().Be(book1.UpdatedAt);

      actualBook1.BookItems.Should().BeEmpty();
   }

   [Fact]
   public void Create_WithTitleSubtitleAndAuthorsText_ShouldTrimValues() {
      // Arrange
      var book1 = _seed.Book1();

      // Act
      var result = Book.Create(
         id: book1.Id,
         authorsText: "   Robert C. Martin   ",
         title: "   Clean Code   ",
         subtitle: "   A Handbook of Agile Software Craftsmanship   ",
         isbn: book1.IsbnVo.Value,
         createdAt: book1.CreatedAt
      );

      // Assert
      result.IsSuccess.Should().BeTrue();

      var actualBook1 = result.Value;

      actualBook1.Id.Should().Be(book1.Id);
      actualBook1.AuthorsText.Should().Be("Robert C. Martin");
      actualBook1.Title.Should().Be(book1.Title);
      actualBook1.Subtitle.Should().Be(book1.Subtitle);
      actualBook1.IsbnVo.Should().Be(book1.IsbnVo);
      actualBook1.CreatedAt.Should().Be(book1.CreatedAt);
      actualBook1.UpdatedAt.Should().Be(book1.CreatedAt);
   }

   [Fact]
   public void Create_WithMultipleAuthors_ShouldNormalizeAuthorsText() {
      // Arrange
      var book1 = _seed.Book1();

      // Act
      var result = Book.Create(
         id: book1.Id,
         authorsText: "   Robert C. Martin   ,   Martin Fowler   ,  Kent Beck  ",
         title: book1.Title,
         subtitle: book1.Subtitle,
         isbn: book1.IsbnVo.Value,
         createdAt: book1.CreatedAt
      );

      // Assert
      result.IsSuccess.Should().BeTrue();

      var actualBook1 = result.Value;

      actualBook1.AuthorsText.Should().Be(
         "Robert C. Martin, Martin Fowler, Kent Beck"
      );
   }

   [Fact]
   public void Create_WithLastnameOnlyAuthor_ShouldReturnSuccess() {
      // Arrange
      var book1 = _seed.Book1();

      // Act
      var result = Book.Create(
         id: book1.Id,
         authorsText: "Martin",
         title: book1.Title,
         subtitle: book1.Subtitle,
         isbn: book1.IsbnVo.Value,
         createdAt: book1.CreatedAt
      );

      // Assert
      result.IsSuccess.Should().BeTrue();

      var actualBook1 = result.Value;

      actualBook1.AuthorsText.Should().Be("Martin");
   }

   [Fact]
   public void Create_WithoutAuthorsText_ShouldReturnFailure() {
      // Arrange
      var book1 = _seed.Book1();

      // Act
      var result = Book.Create(
         id: book1.Id,
         authorsText: "",
         title: book1.Title,
         subtitle: book1.Subtitle,
         isbn: book1.IsbnVo.Value,
         createdAt: book1.CreatedAt
      );

      // Assert
      result.IsFailure.Should().BeTrue();
      result.Error.Should().Be(CatalogErrors.AuthorsAreRequired);
   }

   [Fact]
   public void Create_WithWhitespaceAuthorsText_ShouldReturnFailure() {
      // Arrange
      var book1 = _seed.Book1();

      // Act
      var result = Book.Create(
         id: book1.Id,
         authorsText: "   ",
         title: book1.Title,
         subtitle: book1.Subtitle,
         isbn: book1.IsbnVo.Value,
         createdAt: book1.CreatedAt
      );

      // Assert
      result.IsFailure.Should().BeTrue();
      result.Error.Should().Be(CatalogErrors.AuthorsAreRequired);
   }

   [Fact]
   public void Create_WithOnlyCommaSeparatedEmptyAuthors_ShouldReturnFailure() {
      // Arrange
      var book1 = _seed.Book1();

      // Act
      var result = Book.Create(
         id: book1.Id,
         authorsText: "  ,  ,  ,  ",
         title: book1.Title,
         subtitle: book1.Subtitle,
         isbn: book1.IsbnVo.Value,
         createdAt: book1.CreatedAt
      );

      // Assert
      result.IsFailure.Should().BeTrue();
      result.Error.Should().Be(CatalogErrors.AuthorsAreRequired);
   }

   [Fact]
   public void Create_WithEmptySubtitle_ShouldSetSubtitleToNull() {
      // Arrange
      var book1 = _seed.Book1();

      // Act
      var result = Book.Create(
         id: book1.Id,
         authorsText: book1.AuthorsText,
         title: book1.Title,
         subtitle: "   ",
         isbn: book1.IsbnVo.Value,
         createdAt: book1.CreatedAt
      );

      // Assert
      result.IsSuccess.Should().BeTrue();

      var actualBook1 = result.Value;

      actualBook1.Subtitle.Should().BeNull();
      actualBook1.CreatedAt.Should().Be(book1.CreatedAt);
      actualBook1.UpdatedAt.Should().Be(book1.CreatedAt);
   }

   [Fact]
   public void Create_WithoutTitle_ShouldReturnFailure() {
      // Arrange
      var book1 = _seed.Book1();

      // Act
      var result = Book.Create(
         id: book1.Id,
         authorsText: book1.AuthorsText,
         title: " ",
         subtitle: book1.Subtitle,
         isbn: book1.IsbnVo.Value,
         createdAt: book1.CreatedAt
      );

      // Assert
      result.IsFailure.Should().BeTrue();
      result.Error.Should().Be(CatalogErrors.TitleIsRequired);
   }

   [Fact]
   public void Create_WithEmptyId_ShouldReturnFailure() {
      // Arrange
      var book1 = _seed.Book1();

      // Act
      var result = Book.Create(
         id: Guid.Empty,
         authorsText: book1.AuthorsText,
         title: book1.Title,
         subtitle: book1.Subtitle,
         isbn: book1.IsbnVo.Value,
         createdAt: book1.CreatedAt
      );

      // Assert
      result.IsFailure.Should().BeTrue();
      result.Error.Should().Be(CatalogErrors.BookIdRequired);
   }

   [Fact]
   public void Create_WithInvalidIsbn_ShouldReturnFailure() {
      // Arrange
      var book1 = _seed.Book1();

      // Act
      var result = Book.Create(
         id: book1.Id,
         authorsText: book1.AuthorsText,
         title: book1.Title,
         subtitle: book1.Subtitle,
         isbn: "9780132350885",
         createdAt: book1.CreatedAt
      );

      // Assert
      result.IsFailure.Should().BeTrue();
      result.Error.Should().Be(CatalogErrors.IsbnChecksumInvalid);
   }

   [Fact]
   public void AddBookItem_ShouldAddBookItemAndUpdateBook() {
      // Arrange
      var book1 = _seed.Book1();

      var bookItemId = Guid.Parse(_seed.BookItem1Id);
      var updatedAt = book1.CreatedAt.AddDays(1);

      // Act
      var result = book1.AddBookItem(
         bookItemId: bookItemId,
         updatedAt: updatedAt
      );

      // Assert
      result.IsSuccess.Should().BeTrue();

      var actualBookItem = result.Value;

      actualBookItem.Id.Should().Be(bookItemId);
      actualBookItem.BookId.Should().Be(book1.Id);
      actualBookItem.Status.Should().Be(BookItemStatus.Available);

      book1.BookItems.Should().ContainSingle();
      book1.BookItems.Should().Contain(actualBookItem);

      book1.CreatedAt.Should().Be(_seed.Book1().CreatedAt);
      book1.UpdatedAt.Should().Be(updatedAt);
   }
   
   [Fact]
   public void AddBookItem_WithEmptyId_ShouldReturnFailure() {
      // Arrange
      var book1 = _seed.Book1();

      // Act
      var result = book1.AddBookItem(
         bookItemId: Guid.Empty,
         updatedAt: book1.CreatedAt.AddDays(1)
      );

      // Assert
      result.IsFailure.Should().BeTrue();
      result.Error.Should().Be(CatalogErrors.BookItemIdRequired);

      book1.BookItems.Should().BeEmpty();
      book1.UpdatedAt.Should().Be(book1.CreatedAt);
   }

   [Fact]
   public void Deactivate_ShouldSetIsActiveToFalseAndUpdateBook() {
      // Arrange
      var book1 = _seed.Book1();

      var updatedAt = book1.CreatedAt.AddDays(1);

      // Act
      var result = book1.Deactivate(
         updatedAt: updatedAt
      );

      // Assert
      result.IsSuccess.Should().BeTrue();

      book1.IsActive.Should().BeFalse();
      book1.BookItems.Should().BeEmpty();
      book1.CreatedAt.Should().Be(_seed.Book1().CreatedAt);
      book1.UpdatedAt.Should().Be(updatedAt);
   }

   [Fact]
   public void Deactivate_WhenBookIsAlreadyInactive_ShouldReturnSuccessWithoutUpdatingBookAgain() {
      // Arrange
      var book1 = _seed.Book1();

      var firstUpdatedAt = book1.CreatedAt.AddDays(1);
      var secondUpdatedAt = book1.CreatedAt.AddDays(2);

      var firstResult = book1.Deactivate(
         updatedAt: firstUpdatedAt
      );

      firstResult.IsSuccess.Should().BeTrue();

      // Act
      var result = book1.Deactivate(
         updatedAt: secondUpdatedAt
      );

      // Assert
      result.IsSuccess.Should().BeTrue();

      book1.IsActive.Should().BeFalse();
      book1.UpdatedAt.Should().Be(firstUpdatedAt);
   }
}