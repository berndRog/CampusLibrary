
using AwesomeAssertions;
using CampusLibraryApi._3_Core.Catalog._3_Domain.Entities;
using CampusLibraryApi._3_Core.Catalog._3_Domain.Errors;
using CampusLibraryApiTest.TestInfrastructure;
namespace CampusLibraryApiTest._3_Core.Catalog._3_Domain;

public sealed class AuthorUt {
   
   private readonly TestSeed _seed = new TestSeed();

   [Fact]
   public void Create_WithValidAuthor_ShouldReturnSuccess() {
      
      // Arrange
      var author1 = _seed.Author1();

      // Act
      var result = Author.Create(
         id: author1.Id,
         firstname: author1.Firstname,
         lastname: author1.Lastname,
         createdAt: author1.CreatedAt
      );

      // Assert
      result.IsSuccess.Should().BeTrue();
      var actualAuthor1 = result.Value;

      actualAuthor1.Id.Should().Be(author1.Id);
      actualAuthor1.Firstname.Should().Be(author1.Firstname);
      actualAuthor1.Lastname.Should().Be(author1.Lastname);
      actualAuthor1.DisplayName.Should().Be(author1.DisplayName);
      actualAuthor1.CreatedAt.Should().Be(author1.CreatedAt);
      
      actualAuthor1.Should().BeEquivalentTo(author1); //, opt => opt
         //.Excluding(a => a.));
      
   }

   [Fact]
   public void Create_WithFirstnameAndLastname_ShouldTrimValues() {
      // Arrange
      var author1 = _seed.Author1();
      
      // Act
      var result = Author.Create(
         id: author1.Id,
         firstname: " Robert C.       ",
         lastname: "       Martin   ",
         createdAt: author1.CreatedAt
      );
      
      // Assert
      result.IsSuccess.Should().BeTrue();
      var actualAuthor1 = result.Value;
      actualAuthor1.Id.Should().Be(author1.Id);
      actualAuthor1.Firstname.Should().Be(author1.Firstname);
      actualAuthor1.Lastname.Should().Be(author1.Lastname);
      actualAuthor1.DisplayName.Should().Be(author1.DisplayName);
      actualAuthor1.CreatedAt.Should().Be(author1.CreatedAt);
   }

   [Fact]
   public void Create_WithoutFirstnameAndLastname_ShouldReturnFailure() {
      // Arrange
      var author1 = _seed.Author1();
      
      // Act
      var result = Author.Create(
         id: author1.Id,
         firstname: " ",
         lastname: "",
         createdAt: author1.CreatedAt
      );

      // Assert
      result.IsFailure.Should().BeTrue();
      result.Error.Should().Be(CatalogErrors.FirstnameIsRequired);
   }

   // [Fact]
   // public void DisplayName_WithLastnameOnly_ShouldReturnLastname() {
   //    // Arrange
   //    var id = Guid.NewGuid();
   //
   //    // Act
   //    var result = Author.Create(
   //       id,
   //       "",
   //       "Homer"
   //    );
   //
   //    // Assert
   //    result.IsSuccess.Should().BeTrue();
   //    result.Value.DisplayName.Should().Be("Homer");
   // }
}