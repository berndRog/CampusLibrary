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
      var authorRepository = scope.ServiceProvider.GetRequiredService<IAuthorRepository>();
      var readModel = scope.ServiceProvider.GetRequiredService<IBookReadModel>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var authors = seed.Authors;
      var books = seed.BooksWithAuthors(authors);

      var book1 = books[0];
      authorRepository.AddRange(authors);
      bookRepository.AddRange(books);

      await unitOfWork.SaveAllChangesAsync("Books with authors inserted", ct);
      unitOfWork.ClearChangeTracker();

      // Act
      var result = await readModel.FindByIdAsync(book1.Id, ct);

      // Assert
      result.IsSuccess.Should().BeTrue();

      var actualBookDto = result.Value;
      actualBookDto.Should().NotBeNull();

      actualBookDto.Id.Should().Be(book1.Id);
      actualBookDto.Title.Should().Be(book1.Title);
      actualBookDto.Subtitle.Should().Be(book1.Subtitle);
      actualBookDto.Isbn.Should().Be(book1.IsbnVo.Value);
      actualBookDto.IsActive.Should().BeTrue();
      actualBookDto.CreatedAt.Should().Be(book1.CreatedAt);
      actualBookDto.UpdatedAt.Should().Be(book1.UpdatedAt);

      actualBookDto.Authors.Should().HaveCount(book1.Authors.Count);
      actualBookDto.BookItems.Should().HaveCount(book1.BookItems.Count);

      actualBookDto.Authors
         .Select(a => a.Id)
         .Should()
         .BeEquivalentTo(book1.Authors.Select(a => a.Id));

      actualBookDto.BookItems
         .Select(bi => bi.InventoryNumber)
         .Should()
         .BeEquivalentTo(book1.BookItems.Select(bi => bi.InventoryNumber));

      actualBookDto.TotalBookItems.Should().Be(book1.BookItems.Count);

      actualBookDto.AvailableBookItems.Should().Be(
         book1.BookItems.Count(bi => bi.Status == BookItemStatus.Available)
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
   public async Task FindByIdAsync_deactivated_book_returns_failure() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var repository = scope.ServiceProvider.GetRequiredService<IBookRepository>();
      var readModel = scope.ServiceProvider.GetRequiredService<IBookReadModel>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var book1 = seed.Book1();
      repository.Add(book1);
      await unitOfWork.SaveAllChangesAsync("Book1 inserted", ct);

      var resultDeactivated = book1.Deactivate(
         updatedAt: book1.CreatedAt.AddDays(1)
      );
      resultDeactivated.IsSuccess.Should().BeTrue();
      await unitOfWork.SaveAllChangesAsync("Book1 deactivated", ct);
      unitOfWork.ClearChangeTracker();

      // Act
      var result = await readModel.FindByIdAsync(
         id: book1.Id,
         ct: ct
      );

      // Assert
      result.IsFailure.Should().BeTrue();
   }

   [Fact]
   public async Task SelectAllAsync_ok() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var bookRepository = scope.ServiceProvider.GetRequiredService<IBookRepository>();
      var authorRepository = scope.ServiceProvider.GetRequiredService<IAuthorRepository>();
      var readModel = scope.ServiceProvider.GetRequiredService<IBookReadModel>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var authors = seed.Authors;

      var books = seed.BooksWithAuthors(
         authors: authors
      );

      authorRepository.AddRange(
         authors: authors
      );

      bookRepository.AddRange(
         books: books
      );

      await unitOfWork.SaveAllChangesAsync(
         "Books with authors inserted",
         ct
      );

      unitOfWork.ClearChangeTracker();

      var expBookIds = books
         .OrderBy(b => b.Title)
         .Select(b => b.Id)
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
         .Select(b => b.Id)
         .ToList();

      actualBookIds.Should().BeEquivalentTo(
         expBookIds,
         options => options.WithStrictOrdering()
      );
   }

   [Fact]
   public async Task SelectAllAsync_does_not_return_deactivated_books() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var bookRepository = scope.ServiceProvider.GetRequiredService<IBookRepository>();
      var authorRepository = scope.ServiceProvider.GetRequiredService<IAuthorRepository>();
      var readModel = scope.ServiceProvider.GetRequiredService<IBookReadModel>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var authors = seed.Authors;

      var books = seed.BooksWithAuthors(
         authors: authors
      );

      var deactivatedBook = books[0];

      authorRepository.AddRange(
         authors: authors
      );

      bookRepository.AddRange(
         books: books
      );

      await unitOfWork.SaveAllChangesAsync(
         "Books with authors inserted",
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

      var actualBookDtos = result.Value;
      actualBookDtos.Should().NotContain(b => b.Id == deactivatedBook.Id);
      actualBookDtos.Count.Should().Be(books.Count - 1);
   }

   [Fact]
   public async Task SearchAsync_by_title_ok() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var bookRepository = scope.ServiceProvider.GetRequiredService<IBookRepository>();
      var authorRepository = scope.ServiceProvider.GetRequiredService<IAuthorRepository>();
      var readModel = scope.ServiceProvider.GetRequiredService<IBookReadModel>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var authors = seed.Authors;

      var books = seed.BooksWithAuthors(
         authors: authors
      );

      var book1 = books[0];

      authorRepository.AddRange(
         authors: authors
      );

      bookRepository.AddRange(
         books: books
      );

      await unitOfWork.SaveAllChangesAsync(
         "Books with authors inserted",
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
      actualBookDtos.Should().ContainSingle(b => b.Id == book1.Id);

      var actualBookDto = actualBookDtos.Single(b => b.Id == book1.Id);

      actualBookDto.Title.Should().Be(book1.Title);
      actualBookDto.Subtitle.Should().Be(book1.Subtitle);
      actualBookDto.Isbn.Should().Be(book1.IsbnVo.Value);
      actualBookDto.Authors.Should().NotBeEmpty();
      actualBookDto.TotalBookItems.Should().Be(book1.BookItems.Count);

      actualBookDto.AvailableBookItems.Should().Be(
         book1.BookItems.Count(bi => bi.Status == BookItemStatus.Available)
      );
   }

   [Fact]
   public async Task SearchAsync_by_isbn_ok() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var bookRepository = scope.ServiceProvider.GetRequiredService<IBookRepository>();
      var authorRepository = scope.ServiceProvider.GetRequiredService<IAuthorRepository>();
      var readModel = scope.ServiceProvider.GetRequiredService<IBookReadModel>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var authors = seed.Authors;

      var books = seed.BooksWithAuthors(
         authors: authors
      );

      var book1 = books[0];

      authorRepository.AddRange(
         authors: authors
      );

      bookRepository.AddRange(
         books: books
      );

      await unitOfWork.SaveAllChangesAsync(
         "Books with authors inserted",
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
      actualBookDto.Isbn.Should().Be(book1.IsbnVo.Value);
   }

   [Fact]
   public async Task SearchAsync_by_author_lastname_ok() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var bookRepository = scope.ServiceProvider.GetRequiredService<IBookRepository>();
      var authorRepository = scope.ServiceProvider.GetRequiredService<IAuthorRepository>();
      var readModel = scope.ServiceProvider.GetRequiredService<IBookReadModel>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var authors = seed.Authors;
      var books = seed.BooksWithAuthors(authors);
      var searchText = authors[0].Lastname;

      authorRepository.AddRange(authors: authors);
      bookRepository.AddRange(books: books);
      await unitOfWork.SaveAllChangesAsync("Books with authors inserted", ct);
      unitOfWork.ClearChangeTracker();

      var expBookIds = books
         .Where(b => b.Authors.Any(a =>
            a.Lastname.Contains(searchText)))
         .Select(b => b.Id)
         .OrderBy(id => id)
         .ToList();

      var search = new BookSearchDto(
         SearchField: BookSearchField.AuthorLastName,
         SearchText: searchText
      );

      // Act
      var result = await readModel.SearchAsync(search, ct);

      // Assert
      result.IsSuccess.Should().BeTrue();

      var actualBookIds = result.Value
         .Select(b => b.Id)
         .OrderBy(id => id)
         .ToList();

      actualBookIds.Should().NotBeEmpty();

      actualBookIds.Should().BeEquivalentTo(
         expBookIds,
         options => options.WithStrictOrdering()
      );
   }

   [Fact]
   public async Task SearchAsync_unknown_title_returns_empty_list() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var bookRepository = scope.ServiceProvider.GetRequiredService<IBookRepository>();
      var authorRepository = scope.ServiceProvider.GetRequiredService<IAuthorRepository>();
      var readModel = scope.ServiceProvider.GetRequiredService<IBookReadModel>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();
         
      // Arrange
      var authors = seed.Authors;

      var books = seed.BooksWithAuthors(
         authors: authors
      );

      authorRepository.AddRange(
         authors: authors
      );

      bookRepository.AddRange(
         books: books
      );

      await unitOfWork.SaveAllChangesAsync(
         "Books with authors inserted",
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
   public async Task SearchAsync_does_not_return_deactivated_books() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var bookRepository = scope.ServiceProvider.GetRequiredService<IBookRepository>();
      var authorRepository = scope.ServiceProvider.GetRequiredService<IAuthorRepository>();
      var readModel = scope.ServiceProvider.GetRequiredService<IBookReadModel>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var authors = seed.Authors;

      var books = seed.BooksWithAuthors(
         authors: authors
      );

      var deactivatedBook = books[0];

      authorRepository.AddRange(
         authors: authors
      );

      bookRepository.AddRange(
         books: books
      );

      await unitOfWork.SaveAllChangesAsync(
         "Books with authors inserted",
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
      result.Value.Should().NotContain(b => b.Id == deactivatedBook.Id);
   }

   [Fact]
   public async Task SelectByAuthorIdAsync_ok() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var bookRepository = scope.ServiceProvider.GetRequiredService<IBookRepository>();
      var authorRepository = scope.ServiceProvider.GetRequiredService<IAuthorRepository>();
      var readModel = scope.ServiceProvider.GetRequiredService<IBookReadModel>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var authors = seed.Authors;

      var books = seed.BooksWithAuthors(
         authors: authors
      );

      var author = authors[0];

      authorRepository.AddRange(
         authors: authors
      );

      bookRepository.AddRange(
         books: books
      );

      await unitOfWork.SaveAllChangesAsync(
         "Books with authors inserted",
         ct
      );

      unitOfWork.ClearChangeTracker();

      var expBookIds = books
         .Where(b => b.Authors.Any(a => a.Id == author.Id))
         .Select(b => b.Id)
         .OrderBy(id => id)
         .ToList();

      // Act
      var result = await readModel.SelectByAuthorIdAsync(
         authorId: author.Id,
         ct: ct
      );

      // Assert
      result.IsSuccess.Should().BeTrue();

      var actualBookIds = result.Value
         .Select(b => b.Id)
         .OrderBy(id => id)
         .ToList();

      actualBookIds.Should().BeEquivalentTo(
         expBookIds,
         options => options.WithStrictOrdering()
      );
   }

   [Fact]
   public async Task SelectByAuthorIdAsync_unknown_author_returns_empty_list() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var readModel = scope.ServiceProvider.GetRequiredService<IBookReadModel>();

      // Arrange
      var unknownAuthorId = Guid.Parse("99999999-0000-0000-0000-000000000000");

      // Act
      var result = await readModel.SelectByAuthorIdAsync(
         authorId: unknownAuthorId,
         ct: ct
      );

      // Assert
      result.IsSuccess.Should().BeTrue();
      result.Value.Should().BeEmpty();
   }
}