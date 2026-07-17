using AwesomeAssertions;
using CampusLibraryApi._2_BuildingBlocks._1_Ports;
using CampusLibraryApi._3_Core.Catalog._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Loans._3_Domain.Errors;
using CampusLibraryApiTest.TestInfrastructure;
using Microsoft.Extensions.DependencyInjection;
using CampusLibraryApi._2_BuildingBlocks._1_Ports.Contracts;
using CampusLibraryApi._2_BuildingBlocks._3_Domain.Errors;
namespace CampusLibraryApiTest._3_InfrastructureTests.Contracts;

public sealed class BookItemLoanContractIntT : TestBaseIntegration {
   public BookItemLoanContractIntT() {
      DbName = nameof(BookItemLoanContractIntT);
      DbMode = DbMode.InMemory;
      SensitiveDataLogging = true;
   }

   [Fact]
   public async Task FindByIdAsync_ok() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var bookRepository = scope.ServiceProvider.GetRequiredService<IBookRepository>();
      var contract = scope.ServiceProvider.GetRequiredService<IBookItemLoanContract>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var books = seed.Books;
      var book1 = books[0];
      var bookItem1 = book1.BookItems.FirstOrDefault()!;

      bookRepository.AddRange(books: books);
      await unitOfWork.SaveAllChangesAsync("Books inserted", ct);
      unitOfWork.ClearChangeTracker();

      // Act
      var result = await contract.FindBookItemForLoanAsync(bookItem1.Id, ct);

      // Assert
      result.IsSuccess.Should().BeTrue();

      var actualBookItemDto = result.Value;

      actualBookItemDto.Should().NotBeNull();
      actualBookItemDto.BookItemId.Should().Be(bookItem1.Id);
      actualBookItemDto.BookId.Should().Be(book1.Id);
      actualBookItemDto.Title.Should().Be(book1.Title);
      actualBookItemDto.AuthorsText.Should().Be(book1.AuthorsText);
      actualBookItemDto.Isbn.Should().Be(book1.IsbnVo.Value);
      actualBookItemDto.BookIsActive.Should().BeTrue();
      actualBookItemDto.IsAvailableForLoan.Should().BeTrue();
   }

   [Fact]
   public async Task FindByIdAsync_empty_id_returns_failure() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var contract = scope.ServiceProvider.GetRequiredService<IBookItemLoanContract>();

      // Act
      var result = await contract.FindBookItemForLoanAsync(Guid.Empty, ct);

      // Assert
      result.IsFailure.Should().BeTrue();
      result.Error.Should().Be(CommonErrors.BookItemIdRequired);
   }

   [Fact]
   public async Task FindByIdAsync_unknown_id_returns_failure() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var contract = scope.ServiceProvider.GetRequiredService<IBookItemLoanContract>();

      // Arrange
      var unknownBookItemId = Guid.Parse("be999999-0000-0000-0000-000000000000");

      // Act
      var result = await contract.FindBookItemForLoanAsync(unknownBookItemId, ct);

      // Assert
      result.IsFailure.Should().BeTrue();
      result.Error.Should().Be(CommonErrors.BookItemNotFound);
   }

   [Fact]
   public async Task FindByIdAsync_deactivated_book_item_returns_not_found() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var bookRepository = scope.ServiceProvider.GetRequiredService<IBookRepository>();
      var contract = scope.ServiceProvider.GetRequiredService<IBookItemLoanContract>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var books = seed.Books;
      var book1 = books[0];
      var bookItem1 = book1.BookItems.FirstOrDefault()!;

      bookRepository.AddRange(books);
      await unitOfWork.SaveAllChangesAsync("Books inserted", ct);

      var resultDeactivated = book1.Deactivate(
         updatedAt: book1.CreatedAt.AddDays(1)
      );

      resultDeactivated.IsSuccess.Should().BeTrue();

      await unitOfWork.SaveAllChangesAsync("Book deactivated", ct);
      unitOfWork.ClearChangeTracker();

      // Act
      var result = await contract.FindBookItemForLoanAsync(bookItem1.Id, ct);

      // Assert
      result.IsFailure.Should().BeTrue();
      result.Error.Should().Be(CommonErrors.BookItemNotFound);
   }
}
