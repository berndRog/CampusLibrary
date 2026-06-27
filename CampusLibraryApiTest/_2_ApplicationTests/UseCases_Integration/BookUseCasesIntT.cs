using AwesomeAssertions;
using CampusLibraryApi._2_BuildingBlocks._1_Ports;
using CampusLibraryApi._3_Core.Catalog._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Catalog._2_Application.Dtos;
using CampusLibraryApi._3_Core.Catalog._2_Application.UseCases;
using CampusLibraryApi._3_Core.Catalog._3_Domain.Errors;
using CampusLibraryApiTest.TestInfrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace CampusLibraryApiTest._2_ApplicationTests.UseCases_Integration;

public sealed class BookUseCasesIntT : TestBaseIntegration {

   public BookUseCasesIntT() {
      DbName = nameof(BookUseCasesIntT);
      DbMode = DbMode.InMemory;
      SensitiveDataLogging = true;
   }

   #region BookUcCreate
   [Fact]
   public async Task CreateAsync_ok_persists_book_to_database() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var useCases = scope.ServiceProvider.GetRequiredService<IBookUseCases>();
      var readModel = scope.ServiceProvider.GetRequiredService<IBookReadModel>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var book1 = seed.Book1();

      var dto = new BookCreateDto(
         Title: book1.Title,
         Subtitle: book1.Subtitle,
         Isbn: book1.IsbnVo.Value,
         AuthorsText: book1.AuthorsText,
         Id: book1.Id.ToString()
      );

      // Act
      var resultCreate = await useCases.CreateAsync(
         dto: dto,
         ct: ct
      );

      // Assert
      resultCreate.IsSuccess.Should().BeTrue();

      var createdBookDto = resultCreate.Value;

      var resultFind = await readModel.FindByIdAsync(
         id: createdBookDto.Id,
         ct: ct
      );

      resultFind.IsSuccess.Should().BeTrue();

      var actualBookDetailDto = resultFind.Value;

      actualBookDetailDto.Id.Should().Be(createdBookDto.Id);
      actualBookDetailDto.AuthorsText.Should().Be(createdBookDto.AuthorsText);
      actualBookDetailDto.Title.Should().Be(createdBookDto.Title);
      actualBookDetailDto.Subtitle.Should().Be(createdBookDto.Subtitle);
      actualBookDetailDto.Isbn.Should().Be(createdBookDto.Isbn);
      actualBookDetailDto.IsActive.Should().BeTrue();
   }

   [Fact]
   public async Task CreateAsync_without_authors_text_fails_and_does_not_insert_book() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var useCases = scope.ServiceProvider.GetRequiredService<IBookUseCases>();
      var readModel = scope.ServiceProvider.GetRequiredService<IBookReadModel>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var book1 = seed.Book1();

      var dto = new BookCreateDto(
         Title: book1.Title,
         Subtitle: book1.Subtitle,
         Isbn: book1.IsbnVo.Value,
         AuthorsText: "  ,  ,  ",
         Id: book1.Id.ToString()
      );

      // Act
      var resultCreate = await useCases.CreateAsync(
         dto: dto,
         ct: ct
      );

      // Assert
      resultCreate.IsFailure.Should().BeTrue();
      resultCreate.Error.Should().Be(CatalogErrors.AuthorsAreRequired);

      var resultFind = await readModel.FindByIdAsync(
         id: book1.Id,
         ct: ct
      );

      resultFind.IsFailure.Should().BeTrue();
   }

   [Fact]
   public async Task CreateAsync_duplicate_isbn_fails_and_does_not_insert_book() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var useCases = scope.ServiceProvider.GetRequiredService<IBookUseCases>();
      var repository = scope.ServiceProvider.GetRequiredService<IBookRepository>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var book1 = seed.Book1();
      var book2 = seed.Book2();

      repository.Add(book1);

      await unitOfWork.SaveAllChangesAsync(
         "Book1 inserted",
         ct
      );

      unitOfWork.ClearChangeTracker();

      var dto = new BookCreateDto(
         Title: book2.Title,
         Subtitle: book2.Subtitle,
         Isbn: book1.IsbnVo.Value,
         AuthorsText: book2.AuthorsText,
         Id: book2.Id.ToString()
      );

      // Act
      var resultCreate = await useCases.CreateAsync(
         dto: dto,
         ct: ct
      );

      // Assert
      resultCreate.IsFailure.Should().BeTrue();
      resultCreate.Error.Should().Be(CatalogErrors.BookAlreadyExists);
   }
   #endregion

   #region BookUcAddBookItem
   [Fact]
   public async Task AddBookItemAsync_ok_persists_book_item_to_database() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var useCases = scope.ServiceProvider.GetRequiredService<IBookUseCases>();
      var repository = scope.ServiceProvider.GetRequiredService<IBookRepository>();
      var readModel = scope.ServiceProvider.GetRequiredService<IBookReadModel>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var book1 = seed.Book1();

      repository.Add(book1);

      await unitOfWork.SaveAllChangesAsync(
         "Book1 inserted",
         ct
      );

      unitOfWork.ClearChangeTracker();

      var dto = new BookItemAddDto(
         InventoryNumber: "CL-UC-BOOK-0001",
         Id: seed.BookItem1Id
      );

      // Act
      var resultAddBookItem = await useCases.AddBookItemAsync(
         id: book1.Id,
         dto: dto,
         ct: ct
      );

      resultAddBookItem.IsSuccess.Should().BeTrue();

      unitOfWork.ClearChangeTracker();

      // Assert
      var resultFind = await readModel.FindByIdAsync(
         id: book1.Id,
         ct: ct
      );

      resultFind.IsSuccess.Should().BeTrue();

      var actualBookDetailDto = resultFind.Value;

      actualBookDetailDto.BookItems
         .Should()
         .ContainSingle(bi => bi.InventoryNumber == dto.InventoryNumber);

      actualBookDetailDto.TotalItems.Should().Be(1);
      actualBookDetailDto.AvailableItems.Should().Be(1);
   }

   [Fact]
   public async Task AddBookItemAsync_duplicate_inventory_number_fails() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var useCases = scope.ServiceProvider.GetRequiredService<IBookUseCases>();
      var repository = scope.ServiceProvider.GetRequiredService<IBookRepository>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var book1 = seed.Book1();
      var book2 = seed.Book2();

      var resultExistingBookItem = book1.AddBookItem(
         bookItemId: Guid.Parse(seed.BookItem1Id),
         inventoryNumber: "CL-UC-BOOK-0001",
         updatedAt: book1.CreatedAt.AddDays(1)
      );

      resultExistingBookItem.IsSuccess.Should().BeTrue();

      repository.AddRange([book1, book2]);

      await unitOfWork.SaveAllChangesAsync(
         "Book1 and Book2 inserted",
         ct
      );

      unitOfWork.ClearChangeTracker();

      var dto = new BookItemAddDto(
         InventoryNumber: "CL-UC-BOOK-0001",
         Id: seed.BookItem2Id
      );

      // Act
      var resultAddBookItem = await useCases.AddBookItemAsync(
         id: book2.Id,
         dto: dto,
         ct: ct
      );

      // Assert
      resultAddBookItem.IsFailure.Should().BeTrue();
      resultAddBookItem.Error.Should().Be(CatalogErrors.BookItemAlreadyExists);
   }

   [Fact]
   public async Task AddBookItemAsync_unknown_book_fails() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var useCases = scope.ServiceProvider.GetRequiredService<IBookUseCases>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var unknownBookId = Guid.Parse("99000000-0000-0000-0000-000000000000");

      var dto = new BookItemAddDto(
         InventoryNumber: "CL-UC-BOOK-0001",
         Id: seed.BookItem1Id
      );

      // Act
      var resultAddBookItem = await useCases.AddBookItemAsync(
         id: unknownBookId,
         dto: dto,
         ct: ct
      );

      // Assert
      resultAddBookItem.IsFailure.Should().BeTrue();
      resultAddBookItem.Error.Should().Be(CatalogErrors.BookNotFound);
   }
   #endregion

   #region BookUcDeactivate
   [Fact]
   public async Task DeactivateAsync_ok_persists_is_active_false() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var useCases = scope.ServiceProvider.GetRequiredService<IBookUseCases>();
      var repository = scope.ServiceProvider.GetRequiredService<IBookRepository>();
      var readModel = scope.ServiceProvider.GetRequiredService<IBookReadModel>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var book1 = seed.Book1();

      repository.Add(book1);

      await unitOfWork.SaveAllChangesAsync(
         "Book1 inserted",
         ct
      );

      unitOfWork.ClearChangeTracker();

      // Act
      var resultDeactivate = await useCases.DeactivateAsync(
         id: book1.Id,
         ct: ct
      );

      resultDeactivate.IsSuccess.Should().BeTrue();

      unitOfWork.ClearChangeTracker();

      // Assert: repository can still load the aggregate.
      var actualBook = await repository.FindByIdAsync(
         id: book1.Id,
         ct: ct
      );

      actualBook.Should().NotBeNull();
      actualBook!.IsActive.Should().BeFalse();

      // Assert: read model hides inactive books from the normal query side.
      var resultFind = await readModel.FindByIdAsync(
         id: book1.Id,
         ct: ct
      );

      resultFind.IsFailure.Should().BeTrue();
   }

   [Fact]
   public async Task DeactivateAsync_unknown_book_fails() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var useCases = scope.ServiceProvider.GetRequiredService<IBookUseCases>();

      // Arrange
      var unknownBookId = Guid.Parse("99000000-0000-0000-0000-000000000000");

      // Act
      var resultDeactivate = await useCases.DeactivateAsync(
         id: unknownBookId,
         ct: ct
      );

      // Assert
      resultDeactivate.IsFailure.Should().BeTrue();
      resultDeactivate.Error.Should().Be(CatalogErrors.BookNotFound);
   }
   #endregion
}