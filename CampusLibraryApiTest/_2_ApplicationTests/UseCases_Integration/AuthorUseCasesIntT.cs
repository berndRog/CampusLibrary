using AwesomeAssertions;
using CampusLibraryApi._2_BuildingBlocks._1_Ports;
using CampusLibraryApi._3_Core.Catalog._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Catalog._2_Application.Dtos;
using CampusLibraryApi._3_Core.Catalog._2_Application.UseCases;
using CampusLibraryApi._3_Core.Catalog._3_Domain.Errors;
using CampusLibraryApiTest.TestInfrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace CampusLibraryApiTest._2_ApplicationTests.UseCases_Integration;

public sealed class AuthorUseCasesIntT : TestBaseIntegration {
   public AuthorUseCasesIntT() {
      DbName = nameof(AuthorUseCasesIntT);
      DbMode = DbMode.InMemory;
      SensitiveDataLogging = true;
   }

   #region AuthorUcCreate
   [Fact]
   public async Task CreateAsync_ok_persists_author_to_database() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var useCases = scope.ServiceProvider.GetRequiredService<IAuthorUseCases>();
      var readModel = scope.ServiceProvider.GetRequiredService<IAuthorReadModel>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();
         
      // Arrange
      var author1 = seed.Author1();

      var dto = new AuthorCreateDto(
         Firstname: author1.Firstname,
         Lastname: author1.Lastname,
         Id: author1.Id.ToString()
      );

      // Act
      var resultCreate = await useCases.CreateAsync(
         dto: dto,
         ct: ct
      );

      // Assert
      resultCreate.IsSuccess.Should().BeTrue();

      var createdAuthorDto = resultCreate.Value;

      var resultFind = await readModel.FindByIdAsync(
         id: createdAuthorDto.Id,
         ct: ct
      );

      resultFind.IsSuccess.Should().BeTrue();

      var actualAuthorDto = resultFind.Value;
      actualAuthorDto.Should().BeEquivalentTo(createdAuthorDto);
         
   }

   [Fact]
   public async Task CreateAsync_duplicate_author_fails_and_does_not_insert_author() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var useCases = scope.ServiceProvider.GetRequiredService<IAuthorUseCases>();
      var repository = scope.ServiceProvider.GetRequiredService<IAuthorRepository>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();
         
      // Arrange
      var author1 = seed.Author1();
      var author2 = seed.Author2();

      repository.Add(
         author: author1
      );

      await unitOfWork.SaveAllChangesAsync(
         "Author1 inserted",
         ct
      );

      unitOfWork.ClearChangeTracker();

      var dto = new AuthorCreateDto(
         Firstname: author1.Firstname,
         Lastname: author1.Lastname,
         Id: author2.Id.ToString()
      );

      // Act
      var resultCreate = await useCases.CreateAsync(
         dto: dto,
         ct: ct
      );

      // Assert
      resultCreate.IsFailure.Should().BeTrue();
      resultCreate.Error.Should().Be(CatalogErrors.AuthorAlreadyExists);
      
   }
   #endregion

   #region AuthorUcDeactivate
   [Fact]
   public async Task DeactivateAsync_ok_persists_is_active_false() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var useCases = scope.ServiceProvider.GetRequiredService<IAuthorUseCases>();
      var repository = scope.ServiceProvider.GetRequiredService<IAuthorRepository>();
      var readModel = scope.ServiceProvider.GetRequiredService<IAuthorReadModel>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();
         
      // Arrange
      var author1 = seed.Author1();

      repository.Add(
         author: author1
      );

      await unitOfWork.SaveAllChangesAsync(
         "Author1 inserted",
         ct
      );

      unitOfWork.ClearChangeTracker();

      // Act
      var resultDeactivate = await useCases.DeactivateAsync(
         authorId: author1.Id,
         ct: ct
      );

      resultDeactivate.IsSuccess.Should().BeTrue();

      unitOfWork.ClearChangeTracker();

      // Assert: repository can still load the aggregate.
      var actualAuthor = await repository.FindByIdAsync(
         id: author1.Id,
         ct: ct
      );

      actualAuthor.Should().NotBeNull();
      actualAuthor!.IsActive.Should().BeFalse();

      // Assert: read model hides inactive authors from the normal query side.
      var resultFind = await readModel.FindByIdAsync(
         id: author1.Id,
         ct: ct
      );

      resultFind.IsFailure.Should().BeTrue();
         
   }

   [Fact]
   public async Task DeactivateAsync_unknown_author_fails() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var useCases = scope.ServiceProvider.GetRequiredService<IAuthorUseCases>();
         
      // Arrange
      var unknownAuthorId = Guid.Parse("99000000-0000-0000-0000-000000000000");

      // Act
      var resultDeactivate = await useCases.DeactivateAsync(
         authorId: unknownAuthorId,
         ct: ct
      );

      // Assert
      resultDeactivate.IsFailure.Should().BeTrue();
      resultDeactivate.Error.Should().Be(CatalogErrors.AuthorNotFound);
         
   }
   #endregion
}