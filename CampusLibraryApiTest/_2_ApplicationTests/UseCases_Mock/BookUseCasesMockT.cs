using AwesomeAssertions;
using CampusLibraryApi._2_BuildingBlocks._1_Ports;
using CampusLibraryApi._3_Core.Catalog._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Catalog._2_Application.Dtos;
using CampusLibraryApi._3_Core.Catalog._2_Application.UseCases;
using CampusLibraryApi._3_Core.Catalog._3_Domain.Entities;
using CampusLibraryApi._3_Core.Catalog._3_Domain.Errors;
using CampusLibraryApiTest.TestInfrastructure;
using Microsoft.Extensions.Logging;
using Moq;

namespace CampusLibraryApiTest._2_ApplicationTests.UseCases_Mock;

public sealed class BookUseCasesMockT {
   private static readonly DateTime CreatedAt =
      new(2025, 01, 01, 00, 00, 00, DateTimeKind.Utc);

   #region BookUcCreate
   [Fact]
   public async Task CreateAsync_ok() {
      // Arrange
      var ct = TestContext.Current.CancellationToken;
      var seed = new TestSeed();
      var book1 = seed.Book1();

      var dto = new BookCreateDto(
         Title: book1.Title,
         Subtitle: book1.Subtitle,
         Isbn: book1.IsbnVo.Value,
         AuthorsText: book1.AuthorsText,
         Id: book1.Id.ToString()
      );

      var repository = new Mock<IBookRepository>();
      repository
         .Setup(r => r.ExistsByIsbnAsync(dto.Isbn, ct))
         .ReturnsAsync(false);

      var unitOfWork = new Mock<IUnitOfWork>();
      unitOfWork
         .Setup(u => u.SaveAllChangesAsync("BookUcCreate", ct))
         .ReturnsAsync(1);

      var sut = new BookUcCreate(
         bookRepository: repository.Object,
         unitOfWork: unitOfWork.Object,
         clock: new FakeClock(CreatedAt),
         logger: Mock.Of<ILogger<BookUcCreate>>()
      );

      // Act
      var resultCreate = await sut.ExecuteAsync(
         bookCreateDto: dto,
         ct: ct
      );

      // Assert
      resultCreate.IsSuccess.Should().BeTrue();

      var actualDto = resultCreate.Value;

      actualDto.Id.Should().Be(book1.Id);
      actualDto.AuthorsText.Should().Be(book1.AuthorsText);
      actualDto.Title.Should().Be(book1.Title);
      actualDto.Subtitle.Should().Be(book1.Subtitle);
      actualDto.Isbn.Should().Be(book1.IsbnVo.Value);

      repository.Verify(
         r => r.Add(It.Is<Book>(book =>
            book.Id == book1.Id &&
            book.AuthorsText == book1.AuthorsText &&
            book.Title == book1.Title &&
            book.IsbnVo.Value == book1.IsbnVo.Value &&
            book.IsActive)),
         Times.Once
      );

      unitOfWork.Verify(
         u => u.SaveAllChangesAsync("BookUcCreate", ct),
         Times.Once
      );
   }

   [Fact]
   public async Task CreateAsync_null_dto_fails() {
      // Arrange
      var ct = TestContext.Current.CancellationToken;

      var repository = new Mock<IBookRepository>();
      var unitOfWork = new Mock<IUnitOfWork>();

      var sut = new BookUcCreate(
         bookRepository: repository.Object,
         unitOfWork: unitOfWork.Object,
         clock: new FakeClock(CreatedAt),
         logger: Mock.Of<ILogger<BookUcCreate>>()
      );

      // Act
      var resultCreate = await sut.ExecuteAsync(
         bookCreateDto: null,
         ct: ct
      );

      // Assert
      resultCreate.IsFailure.Should().BeTrue();
      resultCreate.Error.Should().Be(CatalogErrors.BookCreateDtoRequired);

      repository.Verify(
         r => r.ExistsByIsbnAsync(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()
         ),
         Times.Never
      );

      repository.Verify(
         r => r.Add(It.IsAny<Book>()),
         Times.Never
      );

      unitOfWork.Verify(
         u => u.SaveAllChangesAsync(
            It.IsAny<string?>(),
            It.IsAny<CancellationToken>()
         ),
         Times.Never
      );
   }

   [Fact]
   public async Task CreateAsync_without_authors_text_fails() {
      // Arrange
      var ct = TestContext.Current.CancellationToken;
      var seed = new TestSeed();
      var book1 = seed.Book1();

      var dto = new BookCreateDto(
         Title: book1.Title,
         Subtitle: book1.Subtitle,
         Isbn: book1.IsbnVo.Value,
         AuthorsText: "  ,  ,  ",
         Id: book1.Id.ToString()
      );

      var repository = new Mock<IBookRepository>();
      var unitOfWork = new Mock<IUnitOfWork>();

      var sut = new BookUcCreate(
         bookRepository: repository.Object,
         unitOfWork: unitOfWork.Object,
         clock: new FakeClock(CreatedAt),
         logger: Mock.Of<ILogger<BookUcCreate>>()
      );

      // Act
      var resultCreate = await sut.ExecuteAsync(
         bookCreateDto: dto,
         ct: ct
      );

      // Assert
      resultCreate.IsFailure.Should().BeTrue();
      resultCreate.Error.Should().Be(CatalogErrors.AuthorsAreRequired);

      repository.Verify(
         r => r.ExistsByIsbnAsync(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()
         ),
         Times.Never
      );

      repository.Verify(
         r => r.Add(It.IsAny<Book>()),
         Times.Never
      );

      unitOfWork.Verify(
         u => u.SaveAllChangesAsync(
            It.IsAny<string?>(),
            It.IsAny<CancellationToken>()
         ),
         Times.Never
      );
   }

   [Fact]
   public async Task CreateAsync_duplicate_isbn_fails() {
      // Arrange
      var ct = TestContext.Current.CancellationToken;
      var seed = new TestSeed();
      var book1 = seed.Book1();

      var dto = new BookCreateDto(
         Title: book1.Title,
         Subtitle: book1.Subtitle,
         Isbn: book1.IsbnVo.Value,
         AuthorsText: book1.AuthorsText,
         Id: book1.Id.ToString()
      );

      var repository = new Mock<IBookRepository>();
      repository
         .Setup(r => r.ExistsByIsbnAsync(dto.Isbn, ct))
         .ReturnsAsync(true);

      var unitOfWork = new Mock<IUnitOfWork>();

      var sut = new BookUcCreate(
         bookRepository: repository.Object,
         unitOfWork: unitOfWork.Object,
         clock: new FakeClock(CreatedAt),
         logger: Mock.Of<ILogger<BookUcCreate>>()
      );

      // Act
      var resultCreate = await sut.ExecuteAsync(
         bookCreateDto: dto,
         ct: ct
      );

      // Assert
      resultCreate.IsFailure.Should().BeTrue();
      resultCreate.Error.Should().Be(CatalogErrors.BookAlreadyExists);

      repository.Verify(
         r => r.Add(It.IsAny<Book>()),
         Times.Never
      );

      unitOfWork.Verify(
         u => u.SaveAllChangesAsync(
            It.IsAny<string?>(),
            It.IsAny<CancellationToken>()
         ),
         Times.Never
      );
   }

   [Fact]
   public async Task CreateAsync_invalid_id_fails() {
      // Arrange
      var ct = TestContext.Current.CancellationToken;
      var seed = new TestSeed();
      var book1 = seed.Book1();

      var dto = new BookCreateDto(
         Title: book1.Title,
         Subtitle: book1.Subtitle,
         Isbn: book1.IsbnVo.Value,
         AuthorsText: book1.AuthorsText,
         Id: "not-a-guid"
      );

      var repository = new Mock<IBookRepository>();
      var unitOfWork = new Mock<IUnitOfWork>();

      var sut = new BookUcCreate(
         bookRepository: repository.Object,
         unitOfWork: unitOfWork.Object,
         clock: new FakeClock(CreatedAt),
         logger: Mock.Of<ILogger<BookUcCreate>>()
      );

      // Act
      var resultCreate = await sut.ExecuteAsync(
         bookCreateDto: dto,
         ct: ct
      );

      // Assert
      resultCreate.IsFailure.Should().BeTrue();
      resultCreate.Error.Should().Be(CatalogErrors.InvalidBookId);

      repository.Verify(
         r => r.ExistsByIsbnAsync(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()
         ),
         Times.Never
      );

      repository.Verify(
         r => r.Add(It.IsAny<Book>()),
         Times.Never
      );

      unitOfWork.Verify(
         u => u.SaveAllChangesAsync(
            It.IsAny<string?>(),
            It.IsAny<CancellationToken>()
         ),
         Times.Never
      );
   }
   #endregion

   #region BookUcAddBookItem
   [Fact]
   public async Task AddBookItemAsync_ok() {
      // Arrange
      var ct = TestContext.Current.CancellationToken;
      var seed = new TestSeed();
      var book1 = seed.Book1();

      var dto = new BookItemAddDto(
         Id: seed.BookItem1Id
      );

      var repository = new Mock<IBookRepository>();
      repository
         .Setup(r => r.FindByIdAsync(book1.Id, ct))
         .ReturnsAsync(book1);
      
      var unitOfWork = new Mock<IUnitOfWork>();
      unitOfWork
         .Setup(u => u.SaveAllChangesAsync("BookUcAddBookItem", ct))
         .ReturnsAsync(1);

      var sut = new BookUcAddBookItem(
         bookRepository: repository.Object,
         unitOfWork: unitOfWork.Object,
         clock: new FakeClock(CreatedAt.AddDays(1)),
         logger: Mock.Of<ILogger<BookUcAddBookItem>>()
      );

      // Act
      var resultAddBookItem = await sut.ExecuteAsync(
         bookId: book1.Id,
         bookItemAddDto: dto,
         ct: ct
      );

      // Assert
      resultAddBookItem.IsSuccess.Should().BeTrue();
      
      book1.UpdatedAt.Should().Be(CreatedAt.AddDays(1));

      unitOfWork.Verify(
         u => u.SaveAllChangesAsync(
            "BookUcAddBookItem",
            ct
         ),
         Times.Once
      );
   }
   
   [Fact]
   public async Task AddBookItemAsync_book_not_found_fails() {
      // Arrange
      var ct = TestContext.Current.CancellationToken;
      var seed = new TestSeed();
      var book1 = seed.Book1();

      var dto = new BookItemAddDto(seed.BookItem1Id);

      var repository = new Mock<IBookRepository>();
      repository
         .Setup(r => r.FindByIdAsync(book1.Id, ct))
         .ReturnsAsync((Book?)null);

      var unitOfWork = new Mock<IUnitOfWork>();

      var sut = new BookUcAddBookItem(
         bookRepository: repository.Object,
         unitOfWork: unitOfWork.Object,
         clock: new FakeClock(CreatedAt),
         logger: Mock.Of<ILogger<BookUcAddBookItem>>()
      );

      // Act
      var resultAddBookItem = await sut.ExecuteAsync(
         bookId: book1.Id,
         bookItemAddDto: dto,
         ct: ct
      );

      // Assert
      resultAddBookItem.IsFailure.Should().BeTrue();
      resultAddBookItem.Error.Should().Be(CatalogErrors.BookNotFound);


      unitOfWork.Verify(
         u => u.SaveAllChangesAsync(
            It.IsAny<string?>(),
            It.IsAny<CancellationToken>()
         ),
         Times.Never
      );
   }
   #endregion

   #region BookUcDeactivate
   [Fact]
   public async Task DeactivateAsync_ok() {
      // Arrange
      var ct = TestContext.Current.CancellationToken;
      var seed = new TestSeed();
      var book1 = seed.Books.First(
         book => book.Id == Guid.Parse(seed.Book1Id)
      );

      var repository = new Mock<IBookRepository>();
      repository
         .Setup(r => r.FindByIdAsync(book1.Id, ct))
         .ReturnsAsync(book1);

      var loanCatalogContract = new Mock<ILoanCatalogContract>();
      loanCatalogContract
         .Setup(c => c.ExistsForBookItemsAsync(
            It.IsAny<IReadOnlyCollection<Guid>>(),
            ct
         ))
         .ReturnsAsync(false);

      var unitOfWork = new Mock<IUnitOfWork>();
      unitOfWork
         .Setup(u => u.SaveAllChangesAsync("BookUcDeactivate", ct))
         .ReturnsAsync(1);

      var sut = new BookUcDeactivate(
         bookRepository: repository.Object,
         loanCatalogContract: loanCatalogContract.Object,
         unitOfWork: unitOfWork.Object,
         clock: new FakeClock(CreatedAt.AddDays(1)),
         logger: Mock.Of<ILogger<BookUcDeactivate>>()
      );

      // Act
      var resultDeactivate = await sut.ExecuteAsync(
         bookId: book1.Id,
         ct: ct
      );

      // Assert
      resultDeactivate.IsSuccess.Should().BeTrue();
      resultDeactivate.Value.BookItemCount.Should().Be(0);

      book1.IsActive.Should().BeFalse();
      book1.BookItems.Should().BeEmpty();
      book1.UpdatedAt.Should().Be(CreatedAt.AddDays(1));

      unitOfWork.Verify(
         u => u.SaveAllChangesAsync("BookUcDeactivate", ct),
         Times.Once
      );
   }

   [Fact]
   public async Task DeactivateAsync_with_current_loan_fails_and_keeps_book_items() {
      // Arrange
      var ct = TestContext.Current.CancellationToken;
      var seed = new TestSeed();
      var book1 = seed.Books.First(
         book => book.Id == Guid.Parse(seed.Book1Id)
      );
      var expectedBookItemIds = book1.BookItems
         .Select(bookItem => bookItem.Id)
         .ToArray();

      var repository = new Mock<IBookRepository>();
      repository
         .Setup(r => r.FindByIdAsync(book1.Id, ct))
         .ReturnsAsync(book1);

      var loanCatalogContract = new Mock<ILoanCatalogContract>();
      loanCatalogContract
         .Setup(c => c.ExistsForBookItemsAsync(
            It.Is<IReadOnlyCollection<Guid>>(ids =>
               ids.Count == expectedBookItemIds.Length &&
               expectedBookItemIds.All(ids.Contains)
            ),
            ct
         ))
         .ReturnsAsync(true);

      var unitOfWork = new Mock<IUnitOfWork>();

      var sut = new BookUcDeactivate(
         bookRepository: repository.Object,
         loanCatalogContract: loanCatalogContract.Object,
         unitOfWork: unitOfWork.Object,
         clock: new FakeClock(CreatedAt.AddDays(1)),
         logger: Mock.Of<ILogger<BookUcDeactivate>>()
      );

      // Act
      var resultDeactivate = await sut.ExecuteAsync(
         bookId: book1.Id,
         ct: ct
      );

      // Assert
      resultDeactivate.IsFailure.Should().BeTrue();
      resultDeactivate.Error.Should().Be(
         CatalogErrors.BookCannotBeDeactivatedWithLoans
      );

      book1.IsActive.Should().BeTrue();
      book1.BookItems
         .Select(bookItem => bookItem.Id)
         .Should()
         .BeEquivalentTo(expectedBookItemIds);

      unitOfWork.Verify(
         u => u.SaveAllChangesAsync(
            It.IsAny<string?>(),
            It.IsAny<CancellationToken>()
         ),
         Times.Never
      );
   }

   [Fact]
   public async Task DeactivateAsync_book_not_found_fails() {
      // Arrange
      var ct = TestContext.Current.CancellationToken;
      var seed = new TestSeed();
      var book1 = seed.Book1();

      var repository = new Mock<IBookRepository>();
      repository
         .Setup(r => r.FindByIdAsync(book1.Id, ct))
         .ReturnsAsync((Book?)null);

      var loanCatalogContract = new Mock<ILoanCatalogContract>();
      var unitOfWork = new Mock<IUnitOfWork>();

      var sut = new BookUcDeactivate(
         bookRepository: repository.Object,
         loanCatalogContract: loanCatalogContract.Object,
         unitOfWork: unitOfWork.Object,
         clock: new FakeClock(CreatedAt),
         logger: Mock.Of<ILogger<BookUcDeactivate>>()
      );

      // Act
      var resultDeactivate = await sut.ExecuteAsync(
         bookId: book1.Id,
         ct: ct
      );

      // Assert
      resultDeactivate.IsFailure.Should().BeTrue();
      resultDeactivate.Error.Should().Be(CatalogErrors.BookNotFound);

      unitOfWork.Verify(
         u => u.SaveAllChangesAsync(
            It.IsAny<string?>(),
            It.IsAny<CancellationToken>()
         ),
         Times.Never
      );
   }

   [Fact]
   public async Task DeactivateAsync_empty_id_fails() {
      // Arrange
      var ct = TestContext.Current.CancellationToken;

      var repository = new Mock<IBookRepository>();
      var loanCatalogContract = new Mock<ILoanCatalogContract>();
      var unitOfWork = new Mock<IUnitOfWork>();

      var sut = new BookUcDeactivate(
         bookRepository: repository.Object,
         loanCatalogContract: loanCatalogContract.Object,
         unitOfWork: unitOfWork.Object,
         clock: new FakeClock(CreatedAt),
         logger: Mock.Of<ILogger<BookUcDeactivate>>()
      );

      // Act
      var resultDeactivate = await sut.ExecuteAsync(
         bookId: Guid.Empty,
         ct: ct
      );

      // Assert
      resultDeactivate.IsFailure.Should().BeTrue();
      resultDeactivate.Error.Should().Be(CatalogErrors.InvalidBookId);

      repository.Verify(
         r => r.FindByIdAsync(
            It.IsAny<Guid>(),
            It.IsAny<CancellationToken>()
         ),
         Times.Never
      );

      unitOfWork.Verify(
         u => u.SaveAllChangesAsync(
            It.IsAny<string?>(),
            It.IsAny<CancellationToken>()
         ),
         Times.Never
      );
   }
   #endregion
}