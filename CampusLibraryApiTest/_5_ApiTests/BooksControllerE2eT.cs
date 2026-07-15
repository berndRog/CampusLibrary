using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AwesomeAssertions;
using CampusLibraryApi._2_BuildingBlocks._1_Ports;
using CampusLibraryApi._3_Core.Catalog._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Catalog._2_Application.Dtos;
using CampusLibraryApi._3_Core.Catalog._3_Domain.Enums;
using CampusLibraryApi._3_Core.Loans._1_Ports.Outbound;
using CampusLibraryApiTest.TestController;
using CampusLibraryApiTest.TestInfrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace CampusLibraryApiTest._4_WebTests;

public sealed class BooksControllerE2eT : TestBaseEndToEnd {

   protected override string DatabaseName => nameof(BooksControllerE2eT);
   protected override DbMode DbMode => DbMode.InMemory;

   private readonly string _url = "/camplib/v1";
   private readonly CancellationToken _ct = TestContext.Current.CancellationToken;

   private static readonly JsonSerializerOptions _jsonOptions =
      new(JsonSerializerDefaults.Web);

   [Fact]
   public async Task GetByIdAsync_ok() {
      // Arrange
      Guid bookId = default;
      string authorsText = string.Empty;

      await Factory.WithScopeAsync(async sp => {
         var repository = sp.GetRequiredService<IBookRepository>();
         var unitOfWork = sp.GetRequiredService<IUnitOfWork>();
         var seed = sp.GetRequiredService<TestSeed>();

         // seed books with bookItems
         var books = seed.Books;
         var book = books.First();
         bookId = book.Id;
         authorsText = book.AuthorsText;

         repository.Add(book);
         await unitOfWork.SaveAllChangesAsync("Book1 inserted", _ct);
         unitOfWork.ClearChangeTracker();
      });

      // Act
      var response = await Client.GetAsync(
         requestUri: $"{_url}/books/{bookId}",
         cancellationToken: _ct
      );

      var actualBookDto = await response.Content
         .ReadFromJsonAsync<BookDto>(
            options: _jsonOptions,
            cancellationToken: _ct
         );

      // Assert
      response.StatusCode.Should().Be(HttpStatusCode.OK);

      actualBookDto.Should().NotBeNull();
      actualBookDto!.Id.Should().Be(bookId);
      actualBookDto.AuthorsText.Should().Be(authorsText);
      actualBookDto.Title.Should().Be("Clean Code");
      actualBookDto.Isbn.Should().Be("9780132350884");
      actualBookDto.IsActive.Should().BeTrue();

      actualBookDto.TotalItems.Should().BeGreaterThan(0);
      actualBookDto.AvailableItems.Should().BeGreaterThanOrEqualTo(0);
      actualBookDto.BookItems.Should().NotBeEmpty();
   }

   [Fact]
   public async Task GetByIdAsync_deactivated_book_returns_not_found_by_default() {
      // Arrange
      Guid bookId = default;

      await Factory.WithScopeAsync(async sp => {
         var repository = sp.GetRequiredService<IBookRepository>();
         var unitOfWork = sp.GetRequiredService<IUnitOfWork>();
         var seed = sp.GetRequiredService<TestSeed>();

         var book = seed.Book1();
         bookId = book.Id;

         repository.Add(
            book: book
         );

         await unitOfWork.SaveAllChangesAsync(
            "Book1 inserted",
            _ct
         );

         var resultDeactivated = book.Deactivate(
            updatedAt: book.CreatedAt.AddDays(1)
         );

         resultDeactivated.IsSuccess.Should().BeTrue();

         await unitOfWork.SaveAllChangesAsync(
            "Book1 deactivated",
            _ct
         );

         unitOfWork.ClearChangeTracker();
      });

      // Act
      var response = await Client.GetAsync(
         requestUri: $"{_url}/books/{bookId}",
         cancellationToken: _ct
      );

      // Assert
      response.StatusCode.Should().Be(HttpStatusCode.NotFound);
   }

   [Fact]
   public async Task GetByIdAsync_deactivated_book_returns_ok_if_includeInactive_is_true() {
      // Arrange
      Guid bookId = default;

      await Factory.WithScopeAsync(async sp => {
         var repository = sp.GetRequiredService<IBookRepository>();
         var unitOfWork = sp.GetRequiredService<IUnitOfWork>();
         var seed = sp.GetRequiredService<TestSeed>();

         var book = seed.Book1();
         bookId = book.Id;

         repository.Add(
            book: book
         );

         await unitOfWork.SaveAllChangesAsync(
            "Book1 inserted",
            _ct
         );

         var resultDeactivated = book.Deactivate(
            updatedAt: book.CreatedAt.AddDays(1)
         );

         resultDeactivated.IsSuccess.Should().BeTrue();

         await unitOfWork.SaveAllChangesAsync(
            "Book1 deactivated",
            _ct
         );

         unitOfWork.ClearChangeTracker();
      });

      // Act
      var response = await Client.GetAsync(
         requestUri: $"{_url}/books/{bookId}?includeInactive=true",
         cancellationToken: _ct
      );

      var actualBookDto = await response.Content
         .ReadFromJsonAsync<BookDto>(
            options: _jsonOptions,
            cancellationToken: _ct
         );

      // Assert
      response.StatusCode.Should().Be(HttpStatusCode.OK);

      actualBookDto.Should().NotBeNull();
      actualBookDto!.Id.Should().Be(bookId);
      actualBookDto.IsActive.Should().BeFalse();
      actualBookDto.BookItems.Count.Should().Be(0);
   }

   [Fact]
   public async Task GetAllAsync_ok() {
      // Arrange
      List<Guid> expectedBookIds = [];

      await Factory.WithScopeAsync(async sp => {
         var repository = sp.GetRequiredService<IBookRepository>();
         var unitOfWork = sp.GetRequiredService<IUnitOfWork>();
         var seed = sp.GetRequiredService<TestSeed>();

         var books = new[] {
            seed.Book1(),
            seed.Book2(),
            seed.Book3(),
            seed.Book4()
         };

         expectedBookIds = books
            .Select(book => book.Id)
            .OrderBy(id => id)
            .ToList();

         foreach(var book in books) {
            repository.Add(
               book: book
            );
         }

         await unitOfWork.SaveAllChangesAsync(
            "Books inserted",
            _ct
         );

         unitOfWork.ClearChangeTracker();
      });

      // Act
      var response = await Client.GetAsync(
         requestUri: $"{_url}/books",
         cancellationToken: _ct
      );

      var actualBookDtos = await response.Content
         .ReadFromJsonAsync<List<BookDto>>(
            options: _jsonOptions,
            cancellationToken: _ct
         );

      // Assert
      response.StatusCode.Should().Be(HttpStatusCode.OK);

      actualBookDtos.Should().NotBeNull();

      actualBookDtos!
         .Select(book => book.Id)
         .OrderBy(id => id)
         .Should()
         .BeEquivalentTo(
            expectedBookIds,
            options => options.WithStrictOrdering()
         );

      actualBookDtos
         .Should()
         .OnlyContain(book => !string.IsNullOrWhiteSpace(book.AuthorsText));

      actualBookDtos
         .Should()
         .OnlyContain(book => book.IsActive);
   }

   [Fact]
   public async Task GetAllAsync_does_not_return_deactivated_books_by_default() {
      // Arrange
      Guid deactivatedBookId = default;

      await Factory.WithScopeAsync(async sp => {
         var repository = sp.GetRequiredService<IBookRepository>();
         var unitOfWork = sp.GetRequiredService<IUnitOfWork>();
         var seed = sp.GetRequiredService<TestSeed>();

         var book1 = seed.Book1();
         var book2 = seed.Book2();

         deactivatedBookId = book1.Id;

         repository.Add(
            book: book1
         );

         repository.Add(
            book: book2
         );

         await unitOfWork.SaveAllChangesAsync(
            "Books inserted",
            _ct
         );

         var resultDeactivated = book1.Deactivate(
            updatedAt: book1.CreatedAt.AddDays(1)
         );

         resultDeactivated.IsSuccess.Should().BeTrue();

         await unitOfWork.SaveAllChangesAsync(
            "Book1 deactivated",
            _ct
         );

         unitOfWork.ClearChangeTracker();
      });

      // Act
      var response = await Client.GetAsync(
         requestUri: $"{_url}/books",
         cancellationToken: _ct
      );

      var actualBookDtos = await response.Content
         .ReadFromJsonAsync<List<BookDto>>(
            options: _jsonOptions,
            cancellationToken: _ct
         );

      // Assert
      response.StatusCode.Should().Be(HttpStatusCode.OK);

      actualBookDtos.Should().NotBeNull();

      actualBookDtos!
         .Should()
         .NotContain(book => book.Id == deactivatedBookId);

      actualBookDtos
         .Should()
         .OnlyContain(book => book.IsActive);
   }

   [Fact]
   public async Task GetAllAsync_returns_deactivated_books_if_includeInactive_is_true() {
      // Arrange
      Guid deactivatedBookId = default;

      await Factory.WithScopeAsync(async sp => {
         var repository = sp.GetRequiredService<IBookRepository>();
         var unitOfWork = sp.GetRequiredService<IUnitOfWork>();
         var seed = sp.GetRequiredService<TestSeed>();

         var book1 = seed.Book1();
         var book2 = seed.Book2();

         deactivatedBookId = book1.Id;

         repository.Add(
            book: book1
         );

         repository.Add(
            book: book2
         );

         await unitOfWork.SaveAllChangesAsync(
            "Books inserted",
            _ct
         );

         var resultDeactivated = book1.Deactivate(
            updatedAt: book1.CreatedAt.AddDays(1)
         );

         resultDeactivated.IsSuccess.Should().BeTrue();

         await unitOfWork.SaveAllChangesAsync(
            "Book1 deactivated",
            _ct
         );

         unitOfWork.ClearChangeTracker();
      });

      // Act
      var response = await Client.GetAsync(
         requestUri: $"{_url}/books?includeInactive=true",
         cancellationToken: _ct
      );

      var actualBookDtos = await response.Content
         .ReadFromJsonAsync<List<BookDto>>(
            options: _jsonOptions,
            cancellationToken: _ct
         );

      // Assert
      response.StatusCode.Should().Be(HttpStatusCode.OK);

      actualBookDtos.Should().NotBeNull();

      actualBookDtos!
         .Should()
         .Contain(book =>
            book.Id == deactivatedBookId &&
            book.IsActive == false
         );
   }

   [Fact]
   public async Task CreateAsync_ok() {
      // Arrange
      BookCreateDto dto = default!;

      await Factory.WithScopeAsync(sp => {
         var seed = sp.GetRequiredService<TestSeed>();
         var book = seed.Book4();

         dto = new BookCreateDto(
            Title: book.Title,
            Subtitle: book.Subtitle,
            Isbn: book.IsbnVo.Value,
            AuthorsText: book.AuthorsText,
            Id: book.Id.ToString()
         );

         return Task.CompletedTask;
      });

      // Act
      var response = await Client.PostAsJsonAsync(
         requestUri: $"{_url}/books",
         value: dto,
         options: _jsonOptions,
         cancellationToken: _ct
      );

      var actualBookDto = await response.Content
         .ReadFromJsonAsync<BookDto>(
            options: _jsonOptions,
            cancellationToken: _ct
         );

      // Assert
      response.StatusCode.Should().Be(HttpStatusCode.Created);
      response.Headers.Location.Should().NotBeNull();

      actualBookDto.Should().NotBeNull();
      actualBookDto!.Id.Should().Be(Guid.Parse(dto.Id!));
      actualBookDto.AuthorsText.Should().Be(dto.AuthorsText);
      actualBookDto.Title.Should().Be(dto.Title);
      actualBookDto.Subtitle.Should().Be(dto.Subtitle);
      actualBookDto.Isbn.Should().Be(dto.Isbn);
      actualBookDto.TotalItems.Should().Be(0);
      actualBookDto.IsActive.Should().BeTrue();
   }

   [Fact]
   public async Task AddBookItemAsync_ok() {
      // Arrange
      Guid bookId = default;

      await Factory.WithScopeAsync(async sp => {
         var repository = sp.GetRequiredService<IBookRepository>();
         var unitOfWork = sp.GetRequiredService<IUnitOfWork>();
         var seed = sp.GetRequiredService<TestSeed>();

         var book = seed.Book1();
         bookId = book.Id;

         repository.Add(
            book: book
         );

         await unitOfWork.SaveAllChangesAsync(
            "Book1 inserted",
            _ct
         );

         unitOfWork.ClearChangeTracker();
      });

      var dto = new BookItemAddDto("be000001-0000-0000-0000-000000000000");

      // Act
      var response = await Client.PostAsJsonAsync(
         requestUri: $"{_url}/books/{bookId}/items",
         value: dto,
         options: _jsonOptions,
         cancellationToken: _ct
      );

      var actualBookItemDto = await response.Content
         .ReadFromJsonAsync<BookItemDto>(
            options: _jsonOptions,
            cancellationToken: _ct
         );

      // Assert
      response.StatusCode.Should().Be(HttpStatusCode.OK);

      actualBookItemDto.Should().NotBeNull();
      actualBookItemDto!.Id.Should().Be(Guid.Parse(dto.Id!));
      actualBookItemDto.BookId.Should().Be(bookId);
      actualBookItemDto.Status.Should().Be((int)BookItemStatus.Available);
   }

   [Fact]
   public async Task SearchAsync_by_author_last_name_ok() {
      // Arrange
      var bookCleanCode = new BookCreateDto(
         Title: "Clean Code",
         Subtitle: "A Handbook of Agile Software Craftsmanship",
         Isbn: "9780132350884",
         AuthorsText: "Robert C. Martin",
         Id: "b0000001-0000-0000-0000-000000000000"
      );

      var bookRefactoring = new BookCreateDto(
         Title: "Refactoring",
         Subtitle: "Improving the Design of Existing Code",
         Isbn: "9780201485677",
         AuthorsText: "Martin Fowler",
         Id: "b0000003-0000-0000-0000-000000000000"
      );

      var createCleanCodeResponse = await Client.PostAsJsonAsync(
         requestUri: $"{_url}/books",
         value: bookCleanCode,
         options: _jsonOptions,
         cancellationToken: _ct
      );

      var createRefactoringResponse = await Client.PostAsJsonAsync(
         requestUri: $"{_url}/books",
         value: bookRefactoring,
         options: _jsonOptions,
         cancellationToken: _ct
      );

      createCleanCodeResponse.StatusCode.Should().Be(HttpStatusCode.Created);
      createRefactoringResponse.StatusCode.Should().Be(HttpStatusCode.Created);

      // Act
      var response = await Client.GetAsync(
         requestUri: $"{_url}/books/search?searchField=AuthorLastName&searchText=Martin",
         cancellationToken: _ct
      );

      var actualBookDtos = await response.Content
         .ReadFromJsonAsync<List<BookDto>>(
            options: _jsonOptions,
            cancellationToken: _ct
         );

      // Assert
      response.StatusCode.Should().Be(HttpStatusCode.OK);

      actualBookDtos.Should().NotBeNull();

      actualBookDtos!
         .Select(book => book.Id)
         .Should()
         .Contain(Guid.Parse(bookCleanCode.Id!));

      actualBookDtos
         .Select(book => book.Id)
         .Should()
         .NotContain(Guid.Parse(bookRefactoring.Id!));
   }

   [Fact]
   public async Task SearchAsync_returns_deactivated_books_if_includeInactive_is_true() {
      // Arrange
      Guid bookId = default;
      string title = string.Empty;

      await Factory.WithScopeAsync(async sp => {
         var repository = sp.GetRequiredService<IBookRepository>();
         var unitOfWork = sp.GetRequiredService<IUnitOfWork>();
         var seed = sp.GetRequiredService<TestSeed>();

         var book = seed.Book1();

         bookId = book.Id;
         title = book.Title;

         repository.Add(
            book: book
         );

         await unitOfWork.SaveAllChangesAsync(
            "Book1 inserted",
            _ct
         );

         var resultDeactivated = book.Deactivate(
            updatedAt: book.CreatedAt.AddDays(1)
         );

         resultDeactivated.IsSuccess.Should().BeTrue();

         await unitOfWork.SaveAllChangesAsync(
            "Book1 deactivated",
            _ct
         );

         unitOfWork.ClearChangeTracker();
      });

      // Act
      var response = await Client.GetAsync(
         requestUri: $"{_url}/books/search?searchField=Title&searchText={Uri.EscapeDataString(title)}&includeInactive=true",
         cancellationToken: _ct
      );

      var actualBookDtos = await response.Content
         .ReadFromJsonAsync<List<BookDto>>(
            options: _jsonOptions,
            cancellationToken: _ct
         );

      // Assert
      response.StatusCode.Should().Be(HttpStatusCode.OK);

      actualBookDtos.Should().NotBeNull();

      actualBookDtos!
         .Should()
         .Contain(book =>
            book.Id == bookId &&
            book.IsActive == false
         );
   }

   [Fact]
   public async Task DeactivateAsync_ok() {
      // Arrange
      Guid bookId = default;

      await Factory.WithScopeAsync(async sp => {
         var repository = sp.GetRequiredService<IBookRepository>();
         var unitOfWork = sp.GetRequiredService<IUnitOfWork>();
         var seed = sp.GetRequiredService<TestSeed>();

         var book = seed.Book4();
         bookId = book.Id;

         repository.Add(
            book: book
         );

         await unitOfWork.SaveAllChangesAsync(
            "Book4 inserted",
            _ct
         );

         unitOfWork.ClearChangeTracker();
      });

      // Act
      var responseDeactivate = await Client.PatchAsync(
         requestUri: $"{_url}/books/{bookId}/deactivate",
         content: null,
         cancellationToken: _ct
      );

      var actualBookDto = await responseDeactivate.Content
         .ReadFromJsonAsync<BookDto>(
            options: _jsonOptions,
            cancellationToken: _ct
         );

      var responseGet = await Client.GetAsync(
         requestUri: $"{_url}/books/{bookId}",
         cancellationToken: _ct
      );

      var responseGetIncludingInactive = await Client.GetAsync(
         requestUri: $"{_url}/books/{bookId}?includeInactive=true",
         cancellationToken: _ct
      );

      // Assert
      responseDeactivate.StatusCode.Should().Be(HttpStatusCode.OK);

      actualBookDto.Should().NotBeNull();
      actualBookDto!.Id.Should().Be(bookId);
      actualBookDto.IsActive.Should().BeFalse();

      responseGet.StatusCode.Should().Be(HttpStatusCode.NotFound);
      responseGetIncludingInactive.StatusCode.Should().Be(HttpStatusCode.OK);
   }
   [Fact]
   public async Task DeactivateAsync_with_current_loan_returns_conflict() {
      // Arrange
      Guid bookId = default;

      await Factory.WithScopeAsync(async sp => {
         var bookRepository = sp.GetRequiredService<IBookRepository>();
         var loanRepository = sp.GetRequiredService<ILoanRepository>();
         var unitOfWork = sp.GetRequiredService<IUnitOfWork>();
         var seed = sp.GetRequiredService<TestSeed>();

         // book1 with bookitems
         var book = seed.Books.First(b => b.Id == Guid.Parse(seed.Book1Id));
         var loan = seed.Loan1();
         bookId = book.Id;

         bookRepository.Add(book);
         loanRepository.Add(loan);

         await unitOfWork.SaveAllChangesAsync("Book and loan inserted", _ct);
         unitOfWork.ClearChangeTracker();
      });

      // Act
      var responseDeactivate = await Client.PatchAsync(
         requestUri: $"{_url}/books/{bookId}/deactivate",
         content: null,
         cancellationToken: _ct
      );

      // Assert
      responseDeactivate.StatusCode.Should().Be(HttpStatusCode.Conflict);

      await Factory.WithScopeAsync(async sp => {
         var bookRepository = sp.GetRequiredService<IBookRepository>();
         var unitOfWork = sp.GetRequiredService<IUnitOfWork>();

         unitOfWork.ClearChangeTracker();

         var actualBook = await bookRepository.FindByIdAsync(
            id: bookId,
            ct: _ct
         );

         actualBook.Should().NotBeNull();
         actualBook!.IsActive.Should().BeTrue();
         actualBook.BookItems.Should().NotBeEmpty();
      });
   }

}
