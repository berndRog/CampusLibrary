using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;
using CampusLibraryApi._2_BuildingBlocks._1_Ports;
using CampusLibraryApi._3_Core.Catalog._1_Ports;
using CampusLibraryApi._3_Core.Catalog._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Catalog._2_Application.Dtos;
using CampusLibraryApiTest.TestController;
using CampusLibraryApiTest.TestInfrastructure;
using Microsoft.Extensions.DependencyInjection;
namespace CampusLibraryApiTest._4_WebTests;

public sealed class AuthorsControllerE2eT : TestBaseEndToEnd {
   
   protected override string DatabaseName => nameof(AuthorsControllerE2eT);
   protected override DbMode DbMode => DbMode.InMemory;
   
   private readonly string _url = "/camplib/v1";
   private readonly CancellationToken _ct = TestContext.Current.CancellationToken;

   [Fact]
   public async Task GetByIdAsync_ok() {
      // Arrange
      AuthorDto expectedAuthorDto = default!;

      await Factory.WithScopeAsync(async sp => {
         var repository = sp.GetRequiredService<IAuthorRepository>();
         var unitOfWork = sp.GetRequiredService<IUnitOfWork>();
         var seed = sp.GetRequiredService<TestSeed>();

         var author = seed.Author1();

         expectedAuthorDto = new AuthorDto(
            Id: author.Id,
            Firstname: author.Firstname,
            Lastname: author.Lastname,
            DisplayName: author.DisplayName,
            IsActive: author.IsActive
         );

         repository.Add(author);
         await unitOfWork.SaveAllChangesAsync("Author1 inserted", _ct);
         unitOfWork.ClearChangeTracker();
      });

      // Act
      var response = await Client
         .GetAsync($"{_url}/authors/{expectedAuthorDto.Id}", _ct);
      
      var actualAuthorDto = await response.Content
         .ReadFromJsonAsync<AuthorDto>(_ct);

      // Assert
      response.StatusCode.Should().Be(HttpStatusCode.OK);
      actualAuthorDto.Should().NotBeNull();
      actualAuthorDto.Should().BeEquivalentTo(expectedAuthorDto);
   }

   [Fact]
   public async Task GetAllAsync_ok() {
      // Arrange
      List<AuthorDto> expectedAuthorDtos = [];

      await Factory.WithScopeAsync(async sp => {
         var repository = sp.GetRequiredService<IAuthorRepository>();
         var unitOfWork = sp.GetRequiredService<IUnitOfWork>();
         var seed = sp.GetRequiredService<TestSeed>();

         var authors = new[] {
            seed.Author1(),
            seed.Author2(),
            seed.Author3(),
            seed.Author4(),
            seed.Author5()
         };

         expectedAuthorDtos = authors
            .Select(a => new AuthorDto(
               Id: a.Id,
               Firstname: a.Firstname,
               Lastname: a.Lastname,
               DisplayName: a.DisplayName,
               IsActive: a.IsActive
            ))
            .OrderBy(a => a.Lastname)
            .ThenBy(a => a.Firstname)
            .ToList();

         foreach (var author in authors)
            repository.Add(author);

         await unitOfWork.SaveAllChangesAsync("Authors inserted", _ct);
         unitOfWork.ClearChangeTracker();
      });

      // Act
      var response = await Client
         .GetAsync($"{_url}/authors", _ct);
      
      var actualAuthorDtos = await response.Content
         .ReadFromJsonAsync<List<AuthorDto>>(_ct);

      // Assert
      response.StatusCode.Should().Be(HttpStatusCode.OK);
      actualAuthorDtos.Should().NotBeNull();
      actualAuthorDtos!
         .OrderBy(a => a.Lastname)
         .ThenBy(a => a.Firstname)
         .Should()
         .BeEquivalentTo(expectedAuthorDtos);
   }

   [Fact]
   public async Task SearchAsync_by_lastname_ok() {
      // Arrange
      await Factory.WithScopeAsync(async sp => {
         var repository = sp.GetRequiredService<IAuthorRepository>();
         var unitOfWork = sp.GetRequiredService<IUnitOfWork>();
         var seed = sp.GetRequiredService<TestSeed>();

         repository.Add(seed.Author1());   // Robert C. Martin
         repository.Add(seed.Author3());   // Martin Fowler

         await unitOfWork.SaveAllChangesAsync("Authors inserted", _ct);
         unitOfWork.ClearChangeTracker();
      });

      // Act
      var response = await Client
         .GetAsync($"{_url}/authors/search?searchText=Martin", _ct);
      
      var actualAuthorDtos = await response.Content
         .ReadFromJsonAsync<List<AuthorDto>>(_ct);

      // Assert
      response.StatusCode.Should().Be(HttpStatusCode.OK);
      actualAuthorDtos.Should().NotBeNull();
      actualAuthorDtos.Should().HaveCount(1);
      actualAuthorDtos![0].Lastname.Should().Be("Martin");
   }

   [Fact]
   public async Task CreateAsync_ok() {
      // Arrange
      AuthorCreateDto dto = default!;

      await Factory.WithScopeAsync(sp => {
         var seed = sp.GetRequiredService<TestSeed>();
         var author = seed.Author5();

         dto = new AuthorCreateDto(
            Firstname: author.Firstname,
            Lastname: author.Lastname,
            Id: author.Id.ToString()
         );

         return Task.CompletedTask;
      });

      // Act
      var response = await Client
         .PostAsJsonAsync($"{_url}/authors", dto, _ct);
      
      var actualAuthorDto = await response.Content
         .ReadFromJsonAsync<AuthorDto>(_ct);

      // Assert
      response.StatusCode.Should().Be(HttpStatusCode.Created);
      response.Headers.Location.Should().NotBeNull();
      actualAuthorDto.Should().NotBeNull();
      actualAuthorDto!.Id.Should().Be(Guid.Parse(dto.Id!));
      actualAuthorDto.Firstname.Should().Be(dto.Firstname);
      actualAuthorDto.Lastname.Should().Be(dto.Lastname);
      actualAuthorDto.DisplayName.Should().Be($"{dto.Firstname} {dto.Lastname}");
      actualAuthorDto.IsActive.Should().BeTrue();
   }

   [Fact]
   public async Task DeactivateAsync_ok() {
      // Arrange
      Guid authorId = default;

      await Factory.WithScopeAsync(async sp => {
         var repository = sp.GetRequiredService<IAuthorRepository>();
         var unitOfWork = sp.GetRequiredService<IUnitOfWork>();
         var seed = sp.GetRequiredService<TestSeed>();

         var author = seed.Author5();
         authorId = author.Id;

         repository.Add(author);
         await unitOfWork.SaveAllChangesAsync("Author5 inserted", _ct);
         unitOfWork.ClearChangeTracker();
      });

      // Act
      var responseDeactivate = await Client
         .PatchAsync($"{_url}/authors/{authorId}/deactivate", null, _ct);
      
      var actualAuthorDto = await responseDeactivate.Content
         .ReadFromJsonAsync<AuthorDto>(_ct);

      var responseGet = await Client
         .GetAsync($"{_url}/authors/{authorId}", _ct);

      // Assert
      responseDeactivate.StatusCode.Should().Be(HttpStatusCode.OK);
      actualAuthorDto.Should().NotBeNull();
      actualAuthorDto!.Id.Should().Be(authorId);
      actualAuthorDto.IsActive.Should().BeFalse();
      responseGet.StatusCode.Should().Be(HttpStatusCode.NotFound);
   }
}
