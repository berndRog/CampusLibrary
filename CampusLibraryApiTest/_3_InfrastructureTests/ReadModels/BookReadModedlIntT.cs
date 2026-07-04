using AwesomeAssertions;
using CampusLibraryApi._2_BuildingBlocks._1_Ports;
using CampusLibraryApi._3_Core.Catalog._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Catalog._2_Application.Dtos;
using CampusLibraryApi._3_Core.Catalog._2_Application.Enums;
using CampusLibraryApi._3_Core.Catalog._3_Domain.Enums;
using CampusLibraryApiTest.TestInfrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace CampusLibraryApiTest._3_InfrastructureTests.ReadModels;

public sealed class BookReadModelIntT : TestBaseIntegration {
   public BookReadModelIntT() {
      DbName = nameof(BookReadModelIntT);
      DbMode = DbMode.InMemory;
      SensitiveDataLogging = true;
   }

   [Fact]
   public async Task FindByIdAsync_ok() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var bookRepository = scope.ServiceProvider.GetRequiredService<IBookRepository>();
      var readModel = scope.ServiceProvider.GetRequiredService<IBookReadModel>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var books = seed.Books;
      var book1 = books[0];

      bookRepository.AddRange(books: books);
      await unitOfWork.SaveAllChangesAsync("Books inserted", ct);
      unitOfWork.ClearChangeTracker();

      // Act
      var result = await readModel.FindByIdAsync(
         id: book1.Id, 
         includeInactive: false,
         ct: ct
      );

      // Assert
      result.IsSuccess.Should().BeTrue();

      var actualBookDto = result.Value;

      actualBookDto.Should().NotBeNull();
      actualBookDto.Id.Should().Be(book1.Id);
      actualBookDto.AuthorsText.Should().Be(book1.AuthorsText);
      actualBookDto.Title.Should().Be(book1.Title);
      actualBookDto.Subtitle.Should().Be(book1.Subtitle);
      actualBookDto.Isbn.Should().Be(book1.IsbnVo.Value);
      actualBookDto.IsActive.Should().BeTrue();

      actualBookDto.BookItems.Should().HaveCount(book1.BookItems.Count);

      actualBookDto.BookItems
         .Select(item => item.Id)
         .Should()
         .BeEquivalentTo(
            book1.BookItems.Select(item => item.Id)
         );

      actualBookDto.BookItems
         .Select(item => item.Status)
         .Should()
         .BeEquivalentTo(
            book1.BookItems.Select(item => (int)item.Status)
         );

      actualBookDto.TotalItems.Should().Be(book1.BookItems.Count);

      actualBookDto.AvailableItems.Should().Be(
         book1.BookItems.Count(
            item => item.Status == BookItemStatus.Available
         )
      );
   }

   [Fact]
   public async Task FindByIdAsync_unknown_id_returns_failure() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var readModel = scope.ServiceProvider.GetRequiredService<IBookReadModel>();

      // Arrange
      var unknownId = Guid.Parse("99999999-0000-0000-0000-000000000000");

      // Act
      var result = await readModel.FindByIdAsync(
         id: unknownId,
         ct: ct
      );

      // Assert
      result.IsFailure.Should().BeTrue();
   }

   [Fact]
   public async Task FindByIdAsync_deactivated_book_returns_failure_by_default() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var repository = scope.ServiceProvider.GetRequiredService<IBookRepository>();
      var readModel = scope.ServiceProvider.GetRequiredService<IBookReadModel>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var book1 = seed.Book1();

      repository.Add(
         book: book1
      );

      await unitOfWork.SaveAllChangesAsync(
         "Book1 inserted",
         ct
      );

      var resultDeactivated = book1.Deactivate(
         updatedAt: book1.CreatedAt.AddDays(1)
      );

      resultDeactivated.IsSuccess.Should().BeTrue();

      await unitOfWork.SaveAllChangesAsync("Book1 deactivated", ct);
      unitOfWork.ClearChangeTracker();

      // Act
      var result = await readModel.FindByIdAsync(
         id: book1.Id,
         includeInactive: false,
         ct: ct
      );

      // Assert
      result.IsFailure.Should().BeTrue();
   }

   [Fact]
   public async Task FindByIdAsync_deactivated_book_returns_success_if_includeInactive_is_true() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var repository = scope.ServiceProvider.GetRequiredService<IBookRepository>();
      var readModel = scope.ServiceProvider.GetRequiredService<IBookReadModel>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var book1 = seed.Book1();

      repository.Add(
         book: book1
      );

      await unitOfWork.SaveAllChangesAsync(
         "Book1 inserted",
         ct
      );

      var resultDeactivated = book1.Deactivate(
         updatedAt: book1.CreatedAt.AddDays(1)
      );

      resultDeactivated.IsSuccess.Should().BeTrue();

      await unitOfWork.SaveAllChangesAsync(
         "Book1 deactivated",
         ct
      );

      unitOfWork.ClearChangeTracker();

      // Act
      var result = await readModel.FindByIdAsync(
         id: book1.Id,
         includeInactive: true,
         ct: ct
      );

      // Assert
      result.IsSuccess.Should().BeTrue();
      result.Value.Id.Should().Be(book1.Id);
      result.Value.IsActive.Should().BeFalse();
   }

   [Fact]
   public async Task SelectAllAsync_ok() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var bookRepository = scope.ServiceProvider.GetRequiredService<IBookRepository>();
      var readModel = scope.ServiceProvider.GetRequiredService<IBookReadModel>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var books = seed.Books;

      bookRepository.AddRange(
         books: books
      );

      await unitOfWork.SaveAllChangesAsync(
         "Books inserted",
         ct
      );

      unitOfWork.ClearChangeTracker();

      var expectedBookIds = books
         .OrderBy(book => book.Title)
         .ThenBy(book => book.Subtitle)
         .Select(book => book.Id)
         .ToList();

      // Act
      var result = await readModel.SelectAllAsync(
         ct: ct
      );

      // Assert
      result.IsSuccess.Should().BeTrue();

      var actualBookDtos = result.Value;

      actualBookDtos.Should().NotBeNull();
      actualBookDtos.Count.Should().Be(books.Count);

      var actualBookIds = actualBookDtos
         .Select(book => book.Id)
         .ToList();

      actualBookIds.Should().BeEquivalentTo(
         expectedBookIds,
         options => options.WithStrictOrdering()
      );

      actualBookDtos.Should().OnlyContain(book =>
         !string.IsNullOrWhiteSpace(book.AuthorsText)
      );

      actualBookDtos.Should().OnlyContain(book =>
         book.IsActive
      );
   }

   [Fact]
   public async Task SelectAllAsync_does_not_return_deactivated_books_by_default() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var bookRepository = scope.ServiceProvider.GetRequiredService<IBookRepository>();
      var readModel = scope.ServiceProvider.GetRequiredService<IBookReadModel>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var books = seed.Books;
      var deactivatedBook = books[0];

      bookRepository.AddRange(
         books: books
      );

      await unitOfWork.SaveAllChangesAsync(
         "Books inserted",
         ct
      );

      var resultDeactivated = deactivatedBook.Deactivate(
         updatedAt: deactivatedBook.CreatedAt.AddDays(1)
      );

      resultDeactivated.IsSuccess.Should().BeTrue();

      await unitOfWork.SaveAllChangesAsync(
         "Book deactivated",
         ct
      );

      unitOfWork.ClearChangeTracker();

      // Act
      var result = await readModel.SelectAllAsync(
         ct: ct
      );

      // Assert
      result.IsSuccess.Should().BeTrue();

      result.Value.Should().NotContain(book =>
         book.Id == deactivatedBook.Id
      );

      result.Value.Count.Should().Be(books.Count - 1);
   }

   [Fact]
   public async Task SelectAllAsync_returns_deactivated_books_if_includeInactive_is_true() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var bookRepository = scope.ServiceProvider.GetRequiredService<IBookRepository>();
      var readModel = scope.ServiceProvider.GetRequiredService<IBookReadModel>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var books = seed.Books;
      var deactivatedBook = books[0];

      bookRepository.AddRange(
         books: books
      );

      await unitOfWork.SaveAllChangesAsync(
         "Books inserted",
         ct
      );

      var resultDeactivated = deactivatedBook.Deactivate(
         updatedAt: deactivatedBook.CreatedAt.AddDays(1)
      );

      resultDeactivated.IsSuccess.Should().BeTrue();

      await unitOfWork.SaveAllChangesAsync(
         "Book deactivated",
         ct
      );

      unitOfWork.ClearChangeTracker();

      // Act
      var result = await readModel.SelectAllAsync(
         includeInactive: true,
         ct: ct
      );

      // Assert
      result.IsSuccess.Should().BeTrue();

      result.Value.Should().Contain(book =>
         book.Id == deactivatedBook.Id &&
         book.IsActive == false
      );

      result.Value.Count.Should().Be(books.Count);
   }

   [Fact]
   public async Task SearchAsync_by_title_ok() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var bookRepository = scope.ServiceProvider.GetRequiredService<IBookRepository>();
      var readModel = scope.ServiceProvider.GetRequiredService<IBookReadModel>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var books = seed.Books;
      var book1 = books[0];

      bookRepository.AddRange(
         books: books
      );

      await unitOfWork.SaveAllChangesAsync(
         "Books inserted",
         ct
      );

      unitOfWork.ClearChangeTracker();

      var search = new BookSearchDto(
         SearchField: BookSearchField.Title,
         SearchText: book1.Title
      );

      // Act
      var result = await readModel.SearchAsync(
         search: search,
         ct: ct
      );

      // Assert
      result.IsSuccess.Should().BeTrue();

      var actualBookDtos = result.Value;

      actualBookDtos.Should().NotBeNull();
      actualBookDtos.Should().ContainSingle(book => book.Id == book1.Id);

      var actualBookDto = actualBookDtos.Single(book => book.Id == book1.Id);

      actualBookDto.AuthorsText.Should().Be(book1.AuthorsText);
      actualBookDto.Title.Should().Be(book1.Title);
      actualBookDto.Subtitle.Should().Be(book1.Subtitle);
      actualBookDto.Isbn.Should().Be(book1.IsbnVo.Value);
      actualBookDto.IsActive.Should().BeTrue();

      actualBookDto.TotalItems.Should().Be(book1.BookItems.Count);

      actualBookDto.AvailableItems.Should().Be(
         book1.BookItems.Count(
            item => item.Status == BookItemStatus.Available
         )
      );
   }

   [Fact]
   public async Task SearchAsync_by_isbn_ok() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var bookRepository = scope.ServiceProvider.GetRequiredService<IBookRepository>();
      var readModel = scope.ServiceProvider.GetRequiredService<IBookReadModel>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var books = seed.Books;
      var book1 = books[0];

      bookRepository.AddRange(
         books: books
      );

      await unitOfWork.SaveAllChangesAsync(
         "Books inserted",
         ct
      );

      unitOfWork.ClearChangeTracker();

      var search = new BookSearchDto(
         SearchField: BookSearchField.Isbn,
         SearchText: book1.IsbnVo.Value
      );

      // Act
      var result = await readModel.SearchAsync(
         search: search,
         ct: ct
      );

      // Assert
      result.IsSuccess.Should().BeTrue();

      var actualBookDtos = result.Value;

      actualBookDtos.Should().NotBeNull();
      actualBookDtos.Should().ContainSingle();

      var actualBookDto = actualBookDtos.Single();

      actualBookDto.Id.Should().Be(book1.Id);
      actualBookDto.AuthorsText.Should().Be(book1.AuthorsText);
      actualBookDto.Isbn.Should().Be(book1.IsbnVo.Value);
      actualBookDto.IsActive.Should().BeTrue();
   }

   [Fact]
   public async Task SearchAsync_by_author_lastname_ok() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var bookRepository = scope.ServiceProvider.GetRequiredService<IBookRepository>();
      var readModel = scope.ServiceProvider.GetRequiredService<IBookReadModel>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var books = seed.Books;
      var searchText = "Martin";

      bookRepository.AddRange(
         books: books
      );

      await unitOfWork.SaveAllChangesAsync(
         "Books inserted",
         ct
      );

      unitOfWork.ClearChangeTracker();

      var expectedBookIds = books
         .Where(book => ContainsAuthorLastname(
            authorsText: book.AuthorsText,
            searchText: searchText
         ))
         .Select(book => book.Id)
         .OrderBy(id => id)
         .ToList();

      var search = new BookSearchDto(
         SearchField: BookSearchField.AuthorLastName,
         SearchText: searchText
      );

      // Act
      var result = await readModel.SearchAsync(
         search: search,
         ct: ct
      );

      // Assert
      result.IsSuccess.Should().BeTrue();

      var actualBookIds = result.Value
         .Select(book => book.Id)
         .OrderBy(id => id)
         .ToList();

      actualBookIds.Should().NotBeEmpty();

      actualBookIds.Should().BeEquivalentTo(
         expectedBookIds,
         options => options.WithStrictOrdering()
      );
   }

   [Fact]
   public async Task SearchAsync_by_author_lastname_does_not_match_firstname() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var bookRepository = scope.ServiceProvider.GetRequiredService<IBookRepository>();
      var readModel = scope.ServiceProvider.GetRequiredService<IBookReadModel>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var books = seed.Books;

      bookRepository.AddRange(
         books: books
      );

      await unitOfWork.SaveAllChangesAsync(
         "Books inserted",
         ct
      );

      unitOfWork.ClearChangeTracker();

      var search = new BookSearchDto(
         SearchField: BookSearchField.AuthorLastName,
         SearchText: "Robert"
      );

      // Act
      var result = await readModel.SearchAsync(
         search: search,
         ct: ct
      );

      // Assert
      result.IsSuccess.Should().BeTrue();
      result.Value.Should().BeEmpty();
   }

   [Fact]
   public async Task SearchAsync_unknown_title_returns_empty_list() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var bookRepository = scope.ServiceProvider.GetRequiredService<IBookRepository>();
      var readModel = scope.ServiceProvider.GetRequiredService<IBookReadModel>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var books = seed.Books;

      bookRepository.AddRange(
         books: books
      );

      await unitOfWork.SaveAllChangesAsync(
         "Books inserted",
         ct
      );

      unitOfWork.ClearChangeTracker();

      var search = new BookSearchDto(
         SearchField: BookSearchField.Title,
         SearchText: "Unknown Book Title"
      );

      // Act
      var result = await readModel.SearchAsync(
         search: search,
         ct: ct
      );

      // Assert
      result.IsSuccess.Should().BeTrue();
      result.Value.Should().BeEmpty();
   }

   [Fact]
   public async Task SearchAsync_does_not_return_deactivated_books_by_default() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var bookRepository = scope.ServiceProvider.GetRequiredService<IBookRepository>();
      var readModel = scope.ServiceProvider.GetRequiredService<IBookReadModel>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var books = seed.Books;
      var deactivatedBook = books[0];

      bookRepository.AddRange(
         books: books
      );

      await unitOfWork.SaveAllChangesAsync(
         "Books inserted",
         ct
      );

      var resultDeactivated = deactivatedBook.Deactivate(
         updatedAt: deactivatedBook.CreatedAt.AddDays(1)
      );

      resultDeactivated.IsSuccess.Should().BeTrue();

      await unitOfWork.SaveAllChangesAsync(
         "Book deactivated",
         ct
      );

      unitOfWork.ClearChangeTracker();

      var search = new BookSearchDto(
         SearchField: BookSearchField.Title,
         SearchText: deactivatedBook.Title
      );

      // Act
      var result = await readModel.SearchAsync(
         search: search,
         ct: ct
      );

      // Assert
      result.IsSuccess.Should().BeTrue();

      result.Value.Should().NotContain(book =>
         book.Id == deactivatedBook.Id
      );
   }

   [Fact]
   public async Task SearchAsync_returns_deactivated_books_if_includeInactive_is_true() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var bookRepository = scope.ServiceProvider.GetRequiredService<IBookRepository>();
      var readModel = scope.ServiceProvider.GetRequiredService<IBookReadModel>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var books = seed.Books;
      var deactivatedBook = books[0];

      bookRepository.AddRange(
         books: books
      );

      await unitOfWork.SaveAllChangesAsync(
         "Books inserted",
         ct
      );

      var resultDeactivated = deactivatedBook.Deactivate(
         updatedAt: deactivatedBook.CreatedAt.AddDays(1)
      );

      resultDeactivated.IsSuccess.Should().BeTrue();

      await unitOfWork.SaveAllChangesAsync(
         "Book deactivated",
         ct
      );

      unitOfWork.ClearChangeTracker();

      var search = new BookSearchDto(
         SearchField: BookSearchField.Title,
         SearchText: deactivatedBook.Title
      );

      // Act
      var result = await readModel.SearchAsync(
         search: search,
         includeInactive: true,
         ct: ct
      );

      // Assert
      result.IsSuccess.Should().BeTrue();

      result.Value.Should().Contain(book =>
         book.Id == deactivatedBook.Id &&
         book.IsActive == false
      );
   }

   private static bool ContainsAuthorLastname(
      string authorsText,
      string searchText
   ) {
      if(string.IsNullOrWhiteSpace(authorsText))
         return false;

      if(string.IsNullOrWhiteSpace(searchText))
         return false;

      var normalizedSearchText = Normalize(
         value: searchText
      );

      return ExtractAuthorLastnames(
            authorsText: authorsText
         )
         .Any(lastname =>
            Normalize(
               value: lastname
            ).Contains(normalizedSearchText)
         );
   }

   private static IEnumerable<string> ExtractAuthorLastnames(
      string authorsText
   ) {
      if(string.IsNullOrWhiteSpace(authorsText))
         yield break;

      var authorTokens = authorsText.Split(
         separator: ',',
         options: StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
      );

      foreach(var authorToken in authorTokens) {
         var nameParts = authorToken.Split(
            separator: ' ',
            options: StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
         );

         if(nameParts.Length == 0)
            continue;

         yield return nameParts[^1];
      }
   }

   private static string Normalize(
      string value
   ) =>
      value.Trim().ToLowerInvariant();
}