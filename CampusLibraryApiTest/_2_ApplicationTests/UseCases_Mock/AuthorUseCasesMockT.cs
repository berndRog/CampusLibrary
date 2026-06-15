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

public sealed class AuthorUseCasesMockT {
   private static readonly DateTime CreatedAt =
      new(2025, 01, 01, 00, 00, 00, DateTimeKind.Utc);

   #region AuthorUcCreate
   [Fact]
   public async Task CreateAsync_ok() {
      // Arrange
      var ct = TestContext.Current.CancellationToken;
      var seed = new TestSeed();
      var author1 = seed.Author1();
      var dto = new AuthorCreateDto(
         Firstname: author1.Firstname,
         Lastname: author1.Lastname,
         Id: author1.Id.ToString()
      );

      var repository = new Mock<IAuthorRepository>();
      repository
         .Setup(r => r.ExistsByNameAsync(dto.Firstname, dto.Lastname, ct))
         .ReturnsAsync(false);

      var unitOfWork = new Mock<IUnitOfWork>();
      unitOfWork
         .Setup(u => u.SaveAllChangesAsync("AuthorUcCreate", ct))
         .ReturnsAsync(1);

      var sut = new AuthorUcCreate(
         authorRepository: repository.Object,
         unitOfWork: unitOfWork.Object,
         clock: new FakeClock(CreatedAt),
         logger: Mock.Of<ILogger<AuthorUcCreate>>()
      );

      // Act
      var resultCreate = await sut.ExecuteAsync(
         authorCreateDto: dto,
         ct: ct
      );

      // Assert
      resultCreate.IsSuccess.Should().BeTrue();

      var actualDto = resultCreate.Value;
      actualDto.Id.Should().Be(author1.Id);
      actualDto.Firstname.Should().Be(author1.Firstname);
      actualDto.Lastname.Should().Be(author1.Lastname);

      repository.Verify(
         r => r.Add(It.Is<Author>(author =>
            author.Id == author1.Id &&
            author.Firstname == author1.Firstname &&
            author.Lastname == author1.Lastname &&
            author.IsActive)),
         Times.Once
      );

      unitOfWork.Verify(
         u => u.SaveAllChangesAsync("AuthorUcCreate", ct),
         Times.Once
      );
   }

   [Fact]
   public async Task CreateAsync_duplicate_author_fails() {
      // Arrange
      var ct = TestContext.Current.CancellationToken;
      var seed = new TestSeed();
      var author1 = seed.Author1();

      var dto = new AuthorCreateDto(
         Firstname: author1.Firstname,
         Lastname: author1.Lastname,
         Id: author1.Id.ToString()
      );

      var repository = new Mock<IAuthorRepository>();
      repository
         .Setup(r => r.ExistsByNameAsync(dto.Firstname, dto.Lastname, ct))
         .ReturnsAsync(true);

      var unitOfWork = new Mock<IUnitOfWork>();

      var sut = new AuthorUcCreate(
         authorRepository: repository.Object,
         unitOfWork: unitOfWork.Object,
         clock: new FakeClock(CreatedAt),
         logger: Mock.Of<ILogger<AuthorUcCreate>>()
      );

      // Act
      var resultCreate = await sut.ExecuteAsync(
         authorCreateDto: dto,
         ct: ct
      );

      // Assert
      resultCreate.IsFailure.Should().BeTrue();
      resultCreate.Error.Should().Be(CatalogErrors.AuthorAlreadyExists);

      repository.Verify(
         r => r.Add(It.IsAny<Author>()),
         Times.Never
      );

      unitOfWork.Verify(
         u => u.SaveAllChangesAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()),
         Times.Never
      );
   }

   [Fact]
   public async Task CreateAsync_invalid_id_fails() {
      // Arrange
      var ct = TestContext.Current.CancellationToken;
      var seed = new TestSeed();
      var author1 = seed.Author1();

      var dto = new AuthorCreateDto(
         Firstname: author1.Firstname,
         Lastname: author1.Lastname,
         Id: "not-a-guid"
      );

      var repository = new Mock<IAuthorRepository>();
      var unitOfWork = new Mock<IUnitOfWork>();

      var sut = new AuthorUcCreate(
         authorRepository: repository.Object,
         unitOfWork: unitOfWork.Object,
         clock: new FakeClock(CreatedAt),
         logger: Mock.Of<ILogger<AuthorUcCreate>>()
      );

      // Act
      var resultCreate = await sut.ExecuteAsync(
         authorCreateDto: dto,
         ct: ct
      );

      // Assert
      resultCreate.IsFailure.Should().BeTrue();
      resultCreate.Error.Should().Be(CatalogErrors.InvalidAuthorId);

      repository.Verify(
         r => r.ExistsByNameAsync(
            firstname: It.IsAny<string>(),
            lastname: It.IsAny<string>(),
            ct: It.IsAny<CancellationToken>()
         ),
         Times.Never
      );

      repository.Verify(
         r => r.Add(It.IsAny<Author>()),
         Times.Never
      );

      unitOfWork.Verify(
         u => u.SaveAllChangesAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()),
         Times.Never
      );
   }

   [Fact]
   public async Task CreateAsync_missing_firstname_fails() {
      // Arrange
      var ct = TestContext.Current.CancellationToken;
      var seed = new TestSeed();
      var author1 = seed.Author1();

      var dto = new AuthorCreateDto(
         Firstname: "",
         Lastname: author1.Lastname,
         Id: author1.Id.ToString()
      );

      var repository = new Mock<IAuthorRepository>();
      var unitOfWork = new Mock<IUnitOfWork>();

      var sut = new AuthorUcCreate(
         authorRepository: repository.Object,
         unitOfWork: unitOfWork.Object,
         clock: new FakeClock(CreatedAt),
         logger: Mock.Of<ILogger<AuthorUcCreate>>()
      );

      // Act
      var resultCreate = await sut.ExecuteAsync(
         authorCreateDto: dto,
         ct: ct
      );

      // Assert
      resultCreate.IsFailure.Should().BeTrue();
      resultCreate.Error.Should().Be(CatalogErrors.FirstnameIsRequired);

      repository.Verify(
         r => r.Add(It.IsAny<Author>()),
         Times.Never
      );

      unitOfWork.Verify(
         u => u.SaveAllChangesAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()),
         Times.Never
      );
   }
   #endregion

   #region AuthorUcDeactivate
   [Fact]
   public async Task DeactivateAsync_ok() {
      // Arrange
      var ct = TestContext.Current.CancellationToken;
      var seed = new TestSeed();
      var author1 = seed.Author1();

      var repository = new Mock<IAuthorRepository>();

      repository
         .Setup(r => r.FindByIdAsync(author1.Id, ct))
         .ReturnsAsync(author1);

      var unitOfWork = new Mock<IUnitOfWork>();

      unitOfWork
         .Setup(u => u.SaveAllChangesAsync("AuthorUcDeactivate", ct))
         .ReturnsAsync(1);

      var sut = new AuthorUcDeactivate(
         authorRepository: repository.Object,
         unitOfWork: unitOfWork.Object,
         clock: new FakeClock(CreatedAt.AddDays(1)),
         logger: Mock.Of<ILogger<AuthorUcDeactivate>>()
      );

      // Act
      var resultDeactivate = await sut.ExecuteAsync(
         authorId: author1.Id,
         ct: ct
      );

      // Assert
      resultDeactivate.IsSuccess.Should().BeTrue();

      author1.IsActive.Should().BeFalse();
      author1.UpdatedAt.Should().Be(CreatedAt.AddDays(1));

      unitOfWork.Verify(
         u => u.SaveAllChangesAsync("AuthorUcDeactivate", ct),
         Times.Once
      );
   }

   [Fact]
   public async Task DeactivateAsync_author_not_found_fails() {
      // Arrange
      var ct = TestContext.Current.CancellationToken;
      var seed = new TestSeed();
      var author1 = seed.Author1();

      var repository = new Mock<IAuthorRepository>();
      repository
         .Setup(r => r.FindByIdAsync(author1.Id, ct))
         .ReturnsAsync((Author?)null);

      var unitOfWork = new Mock<IUnitOfWork>();

      var sut = new AuthorUcDeactivate(
         authorRepository: repository.Object,
         unitOfWork: unitOfWork.Object,
         clock: new FakeClock(CreatedAt),
         logger: Mock.Of<ILogger<AuthorUcDeactivate>>()
      );

      // Act
      var resultDeactivate = await sut.ExecuteAsync(
         authorId: author1.Id,
         ct: ct
      );

      // Assert
      resultDeactivate.IsFailure.Should().BeTrue();
      resultDeactivate.Error.Should().Be(CatalogErrors.AuthorNotFound);

      unitOfWork.Verify(
         u => u.SaveAllChangesAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()),
         Times.Never
      );
   }

   [Fact]
   public async Task DeactivateAsync_empty_id_fails() {
      // Arrange
      var ct = TestContext.Current.CancellationToken;

      var repository = new Mock<IAuthorRepository>();
      var unitOfWork = new Mock<IUnitOfWork>();

      var sut = new AuthorUcDeactivate(
         authorRepository: repository.Object,
         unitOfWork: unitOfWork.Object,
         clock: new FakeClock(CreatedAt),
         logger: Mock.Of<ILogger<AuthorUcDeactivate>>()
      );

      // Act
      var resultDeactivate = await sut.ExecuteAsync(
         authorId: Guid.Empty,
         ct: ct
      );

      // Assert
      resultDeactivate.IsFailure.Should().BeTrue();
      resultDeactivate.Error.Should().Be(CatalogErrors.InvalidAuthorId);

      repository.Verify(
         r => r.FindByIdAsync(
            It.IsAny<Guid>(),
            It.IsAny<CancellationToken>()
         ),
         Times.Never
      );

      unitOfWork.Verify(
         u => u.SaveAllChangesAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()),
         Times.Never
      );
   }
   #endregion
}