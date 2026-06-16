using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using AwesomeAssertions;
using CampusLibraryApi._2_BuildingBlocks._1_Ports;
using CampusLibraryApi._3_Core.Catalog._1_Ports;
using CampusLibraryApi._3_Core.Catalog._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Catalog._2_Application.Dtos;
using CampusLibraryApi._3_Core.Catalog._3_Domain.Enums;
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
      new(JsonSerializerDefaults.Web) {
         Converters = {
            new JsonStringEnumConverter()
         }
      };

   [Fact]
   public async Task GetByIdAsync_ok() {
      // Arrange
      Guid bookId = default;

      await Factory.WithScopeAsync(async sp => {
         var repository = sp.GetRequiredService<IBookRepository>();
         var unitOfWork = sp.GetRequiredService<IUnitOfWork>();
         var seed = sp.GetRequiredService<TestSeed>();

         var book = seed.Book1();
         bookId = book.Id;

         repository.Add(book);
         await unitOfWork.SaveAllChangesAsync("Book1 inserted", _ct);
         unitOfWork.ClearChangeTracker();
      });

      // Act
      var response = await Client
         .GetAsync($"{_url}/books/{bookId}", _ct);
      
      var actualBookDto = await response.Content
         .ReadFromJsonAsync<BookDetailDto>(_jsonOptions, _ct);

      // Assert
      response.StatusCode.Should().Be(HttpStatusCode.OK);
      actualBookDto.Should().NotBeNull();
      actualBookDto!.Id.Should().Be(bookId);
      actualBookDto.Title.Should().Be("Clean Code");
      actualBookDto.Isbn.Should().Be("9780132350884");
      actualBookDto.IsActive.Should().BeTrue();
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
            .Select(b => b.Id)
            .OrderBy(id => id)
            .ToList();

         foreach (var book in books)
            repository.Add(book);

         await unitOfWork.SaveAllChangesAsync("Books inserted", _ct);
         unitOfWork.ClearChangeTracker();
      });

      // Act
      var response = await Client
         .GetAsync($"{_url}/books", _ct);
      
      var actualBookDtos = await response.Content
         .ReadFromJsonAsync<List<BookListItemDto>>(_jsonOptions, _ct);

      // Assert
      response.StatusCode.Should().Be(HttpStatusCode.OK);
      actualBookDtos.Should().NotBeNull();
      actualBookDtos!
         .Select(b => b.Id)
         .OrderBy(id => id)
         .Should()
         .BeEquivalentTo(expectedBookIds);
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
            Id: book.Id.ToString()
         );

         return Task.CompletedTask;
      });

      // Act
      var response = await Client
         .PostAsJsonAsync($"{_url}/books", dto, _ct);
      
      var actualBookDto = await response.Content
         .ReadFromJsonAsync<BookDto>(_jsonOptions, _ct);

      // Assert
      response.StatusCode.Should().Be(HttpStatusCode.Created);
      response.Headers.Location.Should().NotBeNull();
      actualBookDto.Should().NotBeNull();
      actualBookDto!.Id.Should().Be(Guid.Parse(dto.Id!));
      actualBookDto.Title.Should().Be(dto.Title);
      actualBookDto.Subtitle.Should().Be(dto.Subtitle);
      actualBookDto.Isbn.Should().Be(dto.Isbn);
      actualBookDto.BookItemCount.Should().Be(0);
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

         repository.Add(book);
         await unitOfWork.SaveAllChangesAsync("Book1 inserted", _ct);
         unitOfWork.ClearChangeTracker();
      });

      var dto = new BookItemAddDto(
         InventoryNumber: "CL-BOOK-0001",
         Id: "be000001-0000-0000-0000-000000000000"
      );

      // Act
      var response = await Client
         .PostAsJsonAsync($"{_url}/books/{bookId}/items", dto, _ct);
      
      var actualBookItemDto = await response.Content
         .ReadFromJsonAsync<BookItemDto>(_jsonOptions, _ct);

      // Assert
      response.StatusCode.Should().Be(HttpStatusCode.OK);
      actualBookItemDto.Should().NotBeNull();
      actualBookItemDto!.Id.Should().Be(Guid.Parse(dto.Id!));
      actualBookItemDto.BookId.Should().Be(bookId);
      actualBookItemDto.InventoryNumber.Should().Be(dto.InventoryNumber);
      actualBookItemDto.Status.Should().Be(BookItemStatus.Available);
   }

   [Fact]
   public async Task AssignAuthorAsync_ok() {
      // Arrange
      Guid bookId = default;
      Guid authorId = default;

      await Factory.WithScopeAsync(async sp => {
         var bookRepository = sp.GetRequiredService<IBookRepository>();
         var authorRepository = sp.GetRequiredService<IAuthorRepository>();
         var unitOfWork = sp.GetRequiredService<IUnitOfWork>();
         var seed = sp.GetRequiredService<TestSeed>();

         var book = seed.Book1();
         var author = seed.Author1();

         bookId = book.Id;
         authorId = author.Id;

         bookRepository.Add(book);
         authorRepository.Add(author);
         await unitOfWork.SaveAllChangesAsync("Book1 and Author1 inserted", _ct);
         unitOfWork.ClearChangeTracker();
      });

      var dto = new BookAssignAuthorDto(
         AuthorId: authorId
      );

      // Act
      var responseAssign = await Client
         .PostAsJsonAsync($"{_url}/books/{bookId}/authors", dto, _ct);
      
      var responseGet = await Client
         .GetAsync($"{_url}/books/{bookId}", _ct);
      
      var actualBookDetailDto = await responseGet.Content
         .ReadFromJsonAsync<BookDetailDto>(_jsonOptions, _ct);

      // Assert
      responseAssign.StatusCode.Should().Be(HttpStatusCode.OK);
      responseGet.StatusCode.Should().Be(HttpStatusCode.OK);
      actualBookDetailDto.Should().NotBeNull();
      actualBookDetailDto!.Authors
         .Select(a => a.Id)
         .Should()
         .Contain(authorId);
   }

   [Fact]
   public async Task SearchAsync_by_author_last_name_ok() {
      // Arrange
      var authorMartin = new AuthorCreateDto(
         Firstname: "Robert C.",
         Lastname: "Martin",
         Id: "a0000001-0000-0000-0000-000000000000"
      );

      var authorFowler = new AuthorCreateDto(
         Firstname: "Martin",
         Lastname: "Fowler",
         Id: "a0000003-0000-0000-0000-000000000000"
      );

      var bookCleanCode = new BookCreateDto(
         Title: "Clean Code",
         Subtitle: "A Handbook of Agile Software Craftsmanship",
         Isbn: "9780132350884",
         Id: "b0000001-0000-0000-0000-000000000000"
      );

      var bookRefactoring = new BookCreateDto(
         Title: "Refactoring",
         Subtitle: "Improving the Design of Existing Code",
         Isbn: "9780201485677",
         Id: "b0000003-0000-0000-0000-000000000000"
      );

      var createAuthorMartinResponse = await Client
         .PostAsJsonAsync($"{_url}/authors", authorMartin, _ct);
      var createAuthorFowlerResponse = await Client
         .PostAsJsonAsync($"{_url}/authors", authorFowler, _ct);
      var createCleanCodeResponse = await Client
         .PostAsJsonAsync($"{_url}/books", bookCleanCode, _ct);
      var createRefactoringResponse = await Client
         .PostAsJsonAsync($"{_url}/books", bookRefactoring, _ct);

      createAuthorMartinResponse.StatusCode.Should().Be(HttpStatusCode.Created);
      createAuthorFowlerResponse.StatusCode.Should().Be(HttpStatusCode.Created);
      createCleanCodeResponse.StatusCode.Should().Be(HttpStatusCode.Created);
      createRefactoringResponse.StatusCode.Should().Be(HttpStatusCode.Created);

      var assignMartinResponse = await Client.PostAsJsonAsync(
         $"{_url}/books/{bookCleanCode.Id}/authors",
         new BookAssignAuthorDto(
            AuthorId: Guid.Parse(authorMartin.Id!)
         ),
         _ct
      );

      var assignFowlerResponse = await Client.PostAsJsonAsync(
         $"{_url}/books/{bookRefactoring.Id}/authors",
         new BookAssignAuthorDto(
            AuthorId: Guid.Parse(authorFowler.Id!)
         ),
         _ct
      );

      assignMartinResponse.StatusCode.Should().Be(HttpStatusCode.OK);
      assignFowlerResponse.StatusCode.Should().Be(HttpStatusCode.OK);

      // Act
      var response = await Client
         .GetAsync($"{_url}/books/search?searchField=AuthorLastName&searchText=Martin", _ct);
      
      var actualBookDtos = await response.Content
         .ReadFromJsonAsync<List<BookListItemDto>>(_jsonOptions, _ct);

      // Assert
      response.StatusCode.Should().Be(HttpStatusCode.OK);
      actualBookDtos.Should().NotBeNull();
      actualBookDtos!
         .Select(b => b.Id)
         .Should()
         .BeEquivalentTo([
            Guid.Parse(bookCleanCode.Id!)
         ]);
      actualBookDtos!
         .Select(b => b.Id)
         .Should()
         .NotContain(Guid.Parse(bookRefactoring.Id!));
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

         repository.Add(book);
         await unitOfWork.SaveAllChangesAsync("Book4 inserted", _ct);
         unitOfWork.ClearChangeTracker();
      });

      // Act
      var responseDeactivate = await Client
         .PatchAsync($"{_url}/books/{bookId}/deactivate", null, _ct);
      
      var actualBookDto = await responseDeactivate.Content
         .ReadFromJsonAsync<BookDto>(_jsonOptions, _ct);

      var responseGet = await Client
         .GetAsync($"{_url}/books/{bookId}", _ct);

      // Assert
      responseDeactivate.StatusCode.Should().Be(HttpStatusCode.OK);
      actualBookDto.Should().NotBeNull();
      actualBookDto!.Id.Should().Be(bookId);
      actualBookDto.IsActive.Should().BeFalse();
      responseGet.StatusCode.Should().Be(HttpStatusCode.NotFound);
   }
}