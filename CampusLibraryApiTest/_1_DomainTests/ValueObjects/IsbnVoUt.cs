
using AwesomeAssertions;
using CampusLibraryApi._3_Core.Catalog._3_Domain.Errors;
using CampusLibraryApi._3_Core.Catalog._3_Domain.ValueObjects;
namespace CampusLibraryApiTest._1_DomainTests.ValueObjects;

public sealed class IsbnVoTests {

   [Fact]
   public void Create_WithValidIsbn13_ShouldReturnSuccess() {
      // Arrange
      var isbn = "9780132350884";

      // Act
      var result = IsbnVo.Create(isbn);

      // Assert
      result.IsSuccess.Should().BeTrue();
      result.Value.Value.Should().Be(isbn);
   }

   [Fact]
   public void Create_WithHyphenatedIsbn13_ShouldNormalizeValue() {
      // Arrange
      var isbn = "978-0-13-235088-4";

      // Act
      var result = IsbnVo.Create(isbn);

      // Assert
      result.IsSuccess.Should().BeTrue();
      result.Value.Value.Should().Be("9780132350884");
   }

   [Fact]
   public void Create_WithMissingIsbn_ShouldReturnFailure() {
      // Arrange
      string? isbn = null;

      // Act
      var result = IsbnVo.Create(isbn);

      // Assert
      result.IsFailure.Should().BeTrue();
      result.Error.Should().Be(CatalogErrors.IsbnIsRequired);
   }

   [Fact]
   public void Create_WithTooShortIsbn_ShouldReturnFailure() {
      // Arrange
      var isbn = "978013235088";

      // Act
      var result = IsbnVo.Create(isbn);

      // Assert
      result.IsFailure.Should().BeTrue();
      result.Error.Should().Be(CatalogErrors.IsbnMustHave13Digits);
   }

   [Fact]
   public void Create_WithNonDigitCharacters_ShouldReturnFailure() {
      // Arrange
      var isbn = "978013235088X";

      // Act
      var result = IsbnVo.Create(isbn);

      // Assert
      result.IsFailure.Should().BeTrue();
      result.Error.Should().Be(CatalogErrors.IsbnMustContainOnlyDigits);
   }

   [Fact]
   public void Create_WithInvalidChecksum_ShouldReturnFailure() {
      // Arrange
      var isbn = "9780132350885";

      // Act
      var result = IsbnVo.Create(isbn);

      // Assert
      result.IsFailure.Should().BeTrue();
      result.Error.Should().Be(CatalogErrors.IsbnChecksumInvalid);
   }
}