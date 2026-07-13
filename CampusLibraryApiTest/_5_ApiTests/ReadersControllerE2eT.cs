using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;
using CampusLibraryApi._2_BuildingBlocks._1_Ports;
using CampusLibraryApi._3_Core.Catalog._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Loans._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Readers._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Readers._2_Application.Dtos;
using CampusLibraryApi._3_Core.Readers._2_Application.Mappings;
using CampusLibraryApiTest.TestController;
using CampusLibraryApiTest.TestHelper.Mappings;
using CampusLibraryApiTest.TestInfrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace CampusLibraryApiTest._4_WebTests;

public sealed class ReadersControllerE2eT : TestBaseEndToEnd {
   protected override string DatabaseName => nameof(ReadersControllerE2eT);
   protected override DbMode DbMode => DbMode.InMemory;

   private const string Url = "/camplib/v1";
   private readonly CancellationToken _ct = TestContext.Current.CancellationToken;

   [Fact]
   public async Task GetByIdAsync_ok() {
      // Arrange
      ReaderDto expectedReaderDto = default!;

      await Factory.WithScopeAsync(async sp => {
         var repository = sp.GetRequiredService<IReaderRepository>();
         var unitOfWork = sp.GetRequiredService<IUnitOfWork>();
         var seed = sp.GetRequiredService<TestSeed>();

         var reader = seed.Reader1();
         expectedReaderDto = reader.ToReaderDto();

         repository.Add(reader);
         await unitOfWork.SaveAllChangesAsync("Reader1 inserted", _ct);
         unitOfWork.ClearChangeTracker();
      });

      // Act
      var response = await Client.GetAsync(
         requestUri: $"{Url}/readers/{expectedReaderDto.Id}",
         cancellationToken: _ct
      );

      var actualReaderDto = await response.Content.ReadFromJsonAsync<ReaderDto>(_ct);

      // Assert
      response.StatusCode.Should().Be(HttpStatusCode.OK);
      actualReaderDto.Should().NotBeNull();
      actualReaderDto.Should().BeEquivalentTo(expectedReaderDto);
   }

   [Fact]
   public async Task GetByEmailAsync_ok() {
      // Arrange
      ReaderDto expectedReaderDto = default!;
      string email = string.Empty;

      await Factory.WithScopeAsync(async sp => {
         var repository = sp.GetRequiredService<IReaderRepository>();
         var unitOfWork = sp.GetRequiredService<IUnitOfWork>();
         var seed = sp.GetRequiredService<TestSeed>();

         var reader = seed.Reader2();
         expectedReaderDto = reader.ToReaderDto();
         email = reader.EmailVo.Value;

         repository.Add(reader);
         await unitOfWork.SaveAllChangesAsync("Reader2 inserted", _ct);
         unitOfWork.ClearChangeTracker();
      });

      // Act
      var response = await Client.GetAsync(
         requestUri: $"{Url}/readers/email?email={Uri.EscapeDataString(email)}",
         cancellationToken: _ct
      );

      var actualReaderDto = await response.Content.ReadFromJsonAsync<ReaderDto>(_ct);

      // Assert
      response.StatusCode.Should().Be(HttpStatusCode.OK);
      actualReaderDto.Should().NotBeNull();
      actualReaderDto.Should().BeEquivalentTo(expectedReaderDto);
   }

   [Fact]
   public async Task GetAllAsync_ok() {
      // Arrange
      List<ReaderDto> expectedReaderDtos = [];

      await Factory.WithScopeAsync(async sp => {
         var repository = sp.GetRequiredService<IReaderRepository>();
         var unitOfWork = sp.GetRequiredService<IUnitOfWork>();
         var seed = sp.GetRequiredService<TestSeed>();

         var readers = seed.Readers;
         expectedReaderDtos = readers
            .Select(r => r.ToReaderDto())
            .OrderBy(r => r.Id)
            .ToList();

         repository.AddRange(readers);
         await unitOfWork.SaveAllChangesAsync("Readers inserted", _ct);
         unitOfWork.ClearChangeTracker();
      });

      // Act
      var response = await Client.GetAsync(
         requestUri: $"{Url}/readers",
         cancellationToken: _ct
      );

      var actualReaderDtos = await response.Content.ReadFromJsonAsync<List<ReaderDto>>(_ct);

      // Assert
      response.StatusCode.Should().Be(HttpStatusCode.OK);
      actualReaderDtos.Should().NotBeNull();
      actualReaderDtos!
         .OrderBy(r => r.Id)
         .Should()
         .BeEquivalentTo(expectedReaderDtos);
   }

   [Fact]
   public async Task CreateAsync_endpoint_removed_returns_method_not_allowed() {
      // Arrange
      ReaderCreateDto dto = default!;

      await Factory.WithScopeAsync(sp => {
         var seed = sp.GetRequiredService<TestSeed>();
         dto = Mappings.ToReaderCreateDto(seed.ReaderRegister());
         return Task.CompletedTask;
      });

      // Act
      var response = await Client.PostAsJsonAsync(
         requestUri: $"{Url}/readers",
         value: dto,
         cancellationToken: _ct
      );

      // Assert
      response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
   }

   [Fact]
   public async Task UpdateAsync_endpoint_removed_returns_method_not_allowed() {
      // Arrange
      Guid readerId = default;
      ReaderUpdateMeDto updateMeDto = default!;

      await Factory.WithScopeAsync(async sp => {
         var repository = sp.GetRequiredService<IReaderRepository>();
         var unitOfWork = sp.GetRequiredService<IUnitOfWork>();
         var seed = sp.GetRequiredService<TestSeed>();

         var reader1 = seed.Reader1();
         var reader2 = seed.Reader2();

         readerId = reader1.Id;
         updateMeDto = Mappings.ToReaderUpdateDto(reader2);

         repository.Add(reader1);
         await unitOfWork.SaveAllChangesAsync("Reader1 inserted", _ct);
         unitOfWork.ClearChangeTracker();
      });

      // Act
      var response = await Client.PutAsJsonAsync(
         requestUri: $"{Url}/readers/{readerId}",
         value: updateMeDto,
         cancellationToken: _ct
      );

      // Assert
      response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
   }

   [Fact]
   public async Task DeactivateAsync_without_token_returns_unauthorized() {
      // Arrange
      Guid readerId = await InsertReader3Async();

      // Act
      var response = await Client.DeleteAsync(
         requestUri: $"{Url}/readers/{readerId}",
         cancellationToken: _ct
      );

      // Assert
      response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
   }

   [Fact]
   public async Task DeactivateAsync_with_reader_role_returns_forbidden() {
      // Arrange
      Guid readerId = await InsertReader3Async();

      using var request = CreateAuthenticatedRequest(
         method: HttpMethod.Delete,
         url: $"{Url}/readers/{readerId}",
         roles: "Reader"
      );

      // Act
      var response = await Client.SendAsync(
         request: request,
         cancellationToken: _ct
      );

      // Assert
      response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
   }

   [Fact]
   public async Task DeactivateAsync_with_current_loan_returns_conflict() {
      // Arrange
      Guid readerId = default;

      await Factory.WithScopeAsync(async sp => {
         var readerRepository = sp.GetRequiredService<IReaderRepository>();
         var bookRepository = sp.GetRequiredService<IBookRepository>();
         var loanRepository = sp.GetRequiredService<ILoanRepository>();
         var unitOfWork = sp.GetRequiredService<IUnitOfWork>();
         var seed = sp.GetRequiredService<TestSeed>();

         var reader = seed.Reader1();
         var loan = seed.Loan1();
         var book = seed.Books.Single(candidate =>
            candidate.BookItems.Any(item => item.Id == loan.BookItemId)
         );

         readerId = reader.Id;
         readerRepository.Add(reader);
         bookRepository.Add(book);
         loanRepository.Add(loan);

         await unitOfWork.SaveAllChangesAsync(
            "Reader, book and loan inserted",
            _ct
         );
         unitOfWork.ClearChangeTracker();
      });

      using var request = CreateAuthenticatedRequest(
         method: HttpMethod.Delete,
         url: $"{Url}/readers/{readerId}",
         roles: "Employee"
      );

      // Act
      var response = await Client.SendAsync(
         request: request,
         cancellationToken: _ct
      );

      // Assert
      response.StatusCode.Should().Be(HttpStatusCode.Conflict);
   }

   [Fact]
   public async Task DeactivateAsync_with_employee_role_deactivates_reader() {
      // Arrange
      Guid readerId = await InsertReader3Async();

      using var request = CreateAuthenticatedRequest(
         method: HttpMethod.Delete,
         url: $"{Url}/readers/{readerId}",
         roles: "Employee"
      );

      // Act
      var responseDelete = await Client.SendAsync(
         request: request,
         cancellationToken: _ct
      );

      var responseGet = await Client.GetAsync(
         requestUri: $"{Url}/readers/{readerId}",
         cancellationToken: _ct
      );

      // Assert
      responseDelete.StatusCode.Should().Be(HttpStatusCode.NoContent);
      responseGet.StatusCode.Should().Be(HttpStatusCode.NotFound);
   }

   private async Task<Guid> InsertReader3Async() {
      Guid readerId = default;

      await Factory.WithScopeAsync(async sp => {
         var repository = sp.GetRequiredService<IReaderRepository>();
         var unitOfWork = sp.GetRequiredService<IUnitOfWork>();
         var seed = sp.GetRequiredService<TestSeed>();

         var reader = seed.Reader3();
         readerId = reader.Id;

         repository.Add(reader);
         await unitOfWork.SaveAllChangesAsync("Reader3 inserted", _ct);
         unitOfWork.ClearChangeTracker();
      });

      return readerId;
   }

   private static HttpRequestMessage CreateAuthenticatedRequest(
      HttpMethod method,
      string url,
      string roles
   ) {
      var request = new HttpRequestMessage(
         method: method,
         requestUri: url
      );

      request.Headers.Add(TestAuthHandler.Header, roles);
      request.Headers.Add(TestAuthHandler.SubjectHeader, "employee-subject-e2e-001");
      request.Headers.Add(TestAuthHandler.UsernameHeader, "employee.e2e@example.org");
      request.Headers.Add(TestAuthHandler.CreatedAtHeader, "2025-01-01T00:00:00Z");

      return request;
   }
}
