using AwesomeAssertions;
using CampusLibraryApi._2_BuildingBlocks._1_Ports;
using CampusLibraryApi._3_Core.Catalog._1_Ports.Outbound;
using CampusLibraryApiTest.TestInfrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace CampusLibraryApiTest._3_InfrastructureTests.Repositories;

public sealed class BookRepositoryIntT : TestBaseIntegration {
   public BookRepositoryIntT() {
      DbName = nameof(BookRepositoryIntT);
      DbMode = DbMode.InMemory;
      SensitiveDataLogging = true;
   }

   [Fact]
   public async Task FindByIdAsync_ok() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var bookRepository = scope.ServiceProvider.GetRequiredService<IBookRepository>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var books = seed.Books;
      var book1 = books[0];

      bookRepository.AddRange(books: books);
      await unitOfWork.SaveAllChangesAsync("Books inserted", ct);
      unitOfWork.ClearChangeTracker();

      // Act
      var actualBook = await bookRepository.FindByIdAsync(book1.Id, ct);

      // Assert
      actualBook.Should().NotBeNull();

      actualBook!.Id.Should().Be(book1.Id);
      actualBook.AuthorsText.Should().Be(book1.AuthorsText);
      actualBook.Title.Should().Be(book1.Title);
      actualBook.Subtitle.Should().Be(book1.Subtitle);
      actualBook.IsbnVo.Should().Be(book1.IsbnVo);
      actualBook.CreatedAt.Should().Be(book1.CreatedAt);
      actualBook.UpdatedAt.Should().Be(book1.UpdatedAt);

      // Repository must load child entities needed by domain methods.
      actualBook.BookItems.Should().HaveCount(book1.BookItems.Count);

//    actualBook.BookItems.Should().BeEquivalentTo(book1.BookItems.Select(bi => bi.InventoryNumber));
   }

   [Fact]
   public async Task FindByIdAsync_unknown_id_returns_null() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var repository = scope.ServiceProvider.GetRequiredService<IBookRepository>();

      // Arrange
      var unknownId = Guid.Parse("99999999-0000-0000-0000-000000000000");

      // Act
      var actualBook = await repository.FindByIdAsync(
         id: unknownId,
         ct: ct
      );

      // Assert
      actualBook.Should().BeNull();
   }

   [Fact]
   public async Task ExistsByIsbnAsync_existing_isbn_returns_true() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var repository = scope.ServiceProvider.GetRequiredService<IBookRepository>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var book1 = seed.Book1();
      repository.Add(book1);

      await unitOfWork.SaveAllChangesAsync("Book1 inserted", ct);
      unitOfWork.ClearChangeTracker();

      // Act
      var exists = await repository.ExistsByIsbnAsync(isbn: book1.IsbnVo.Value, ct: ct);

      // Assert
      exists.Should().BeTrue();
   }

   [Fact]
   public async Task ExistsByIsbnAsync_unknown_isbn_returns_false() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var repository = scope.ServiceProvider.GetRequiredService<IBookRepository>();

      // Arrange
      var unknownIsbn = "9780131103627";

      // Act
      var exists = await repository.ExistsByIsbnAsync(isbn: unknownIsbn, ct: ct);

      // Assert
      exists.Should().BeFalse();
   }
   
   [Fact]
   public async Task AddRange_persists_multiple_books_with_items() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var bookRepository = scope.ServiceProvider.GetRequiredService<IBookRepository>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var books = seed.Books;

      bookRepository.AddRange(
         books: books
      );

      var savedRows = await unitOfWork.SaveAllChangesAsync(
         "Books inserted",
         ct
      );

      unitOfWork.ClearChangeTracker();

      // Act
      var actualBook1 = await bookRepository.FindByIdAsync(
         id: books[0].Id,
         ct: ct
      );

      var actualBook2 = await bookRepository.FindByIdAsync(
         id: books[1].Id,
         ct: ct
      );

      var actualBook3 = await bookRepository.FindByIdAsync(
         id: books[2].Id,
         ct: ct
      );

      // Assert
      savedRows.Should().BeGreaterThan(0);

      actualBook1.Should().NotBeNull();
      actualBook2.Should().NotBeNull();
      actualBook3.Should().NotBeNull();

      actualBook1!.Id.Should().Be(books[0].Id);
      actualBook1.AuthorsText.Should().Be(books[0].AuthorsText);

      actualBook2!.Id.Should().Be(books[1].Id);
      actualBook2.AuthorsText.Should().Be(books[1].AuthorsText);

      actualBook3!.Id.Should().Be(books[2].Id);
      actualBook3.AuthorsText.Should().Be(books[2].AuthorsText);

      actualBook1.BookItems.Should().HaveCount(books[0].BookItems.Count);
      actualBook2.BookItems.Should().HaveCount(books[1].BookItems.Count);
      actualBook3.BookItems.Should().HaveCount(books[2].BookItems.Count);
   }

   [Fact]
   public async Task FindByIdAsync_deactivated_book_returns_book_with_is_active_false() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var repository = scope.ServiceProvider.GetRequiredService<IBookRepository>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var book1 = seed.Book1();
      var updatedAt = book1.CreatedAt.AddDays(1);

      repository.Add(book1);

      await unitOfWork.SaveAllChangesAsync(
         "Book1 inserted",
         ct
      );

      var resultDeactivated = book1.Deactivate(
         updatedAt: updatedAt
      );

      resultDeactivated.IsSuccess.Should().BeTrue();

      await unitOfWork.SaveAllChangesAsync(
         "Book1 deactivated",
         ct
      );

      unitOfWork.ClearChangeTracker();

      // Act
      var actualBook = await repository.FindByIdAsync(
         id: book1.Id,
         ct: ct
      );

      // Assert
      actualBook.Should().NotBeNull();
      actualBook!.IsActive.Should().BeFalse();
      actualBook.BookItems.Should().BeEmpty();
      actualBook.UpdatedAt.Should().Be(updatedAt);
   }
}