using AwesomeAssertions;
using CampusLibraryApi._2_BuildingBlocks._1_Ports;
using CampusLibraryApi._3_Core.Loans._3_Domain.Errors;
using CampusLibraryApi._3_Core.Readers._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Readers._2_Application.Dtos;
using CampusLibraryApi._3_Core.Readers._2_Application.UseCases;
using CampusLibraryApi._3_Core.Readers._3_Domain.Entities;
using CampusLibraryApi._3_Core.Readers._3_Domain.Errors;
using CampusLibraryApi._3_Core.Readers._3_Domain.ValueObjects;
using CampusLibraryApiTest.TestInfrastructure;
using Microsoft.Extensions.Logging;
using Moq;

namespace CampusLibraryApiTest._2_ApplicationTests.UseCases_Mock;

public sealed class ReaderProvisionProfileUseCasesMockT {
   private static readonly DateTime CreatedAt =
      new(2025, 01, 01, 00, 00, 00, DateTimeKind.Utc);

   private static readonly DateTime UpdatedAt =
      new(2025, 01, 02, 00, 00, 00, DateTimeKind.Utc);

   private const string Subject = "identity-subject-001";
   private const string Username = "reader.one@example.org";
   private const string ReaderId = "10000000-0000-0000-0000-000000000001";

   [Fact]
   public async Task CreateProvisionAsync_creates_incomplete_reader_from_identity() {
      // Arrange
      var ct = TestContext.Current.CancellationToken;
      var identityGateway = NewReaderIdentity();

      var repository = new Mock<IReaderRepository>();
      repository
         .Setup(r => r.FindBySubjectAsync(Subject, ct))
         .ReturnsAsync((Reader?)null);
      repository
         .Setup(r => r.FindByEmailAsync(It.IsAny<EmailVo>(), ct))
         .ReturnsAsync((Reader?)null);

      var unitOfWork = new Mock<IUnitOfWork>();
      unitOfWork
         .Setup(u => u.SaveAllChangesAsync("ReaderUcCreateProvision", ct))
         .ReturnsAsync(1);

      var sut = new ReaderUcCreateProvision(
         identityGateway: identityGateway,
         repository: repository.Object,
         unitOfWork: unitOfWork.Object,
         logger: Mock.Of<ILogger<ReaderUcCreateProvision>>()
      );

      // Act
      var result = await sut.ExecuteAsync(
         id: ReaderId,
         ct: ct
      );

      // Assert
      result.IsSuccess.Should().BeTrue();
      result.Value.Id.Should().Be(Guid.Parse(ReaderId));
      result.Value.WasCreated.Should().BeTrue();

      repository.Verify(
         r => r.Add(It.Is<Reader>(reader =>
            reader.Id == Guid.Parse(ReaderId) &&
            reader.Subject == Subject &&
            reader.EmailVo.Value == Username &&
            reader.CreatedAt == CreatedAt &&
            reader.AddressVo == null &&
            !reader.IsProfileCompleted
         )),
         Times.Once
      );
      unitOfWork.Verify(
         u => u.SaveAllChangesAsync("ReaderUcCreateProvision", ct),
         Times.Once
      );
   }

   [Fact]
   public async Task CreateProvisionAsync_existing_subject_is_idempotent() {
      // Arrange
      var ct = TestContext.Current.CancellationToken;
      var reader = NewProvisionedReader();

      var repository = new Mock<IReaderRepository>();
      repository
         .Setup(r => r.FindBySubjectAsync(Subject, ct))
         .ReturnsAsync(reader);

      var unitOfWork = new Mock<IUnitOfWork>();

      var sut = new ReaderUcCreateProvision(
         identityGateway: NewReaderIdentity(),
         repository: repository.Object,
         unitOfWork: unitOfWork.Object,
         logger: Mock.Of<ILogger<ReaderUcCreateProvision>>()
      );

      // Act
      var result = await sut.ExecuteAsync(
         id: null,
         ct: ct
      );

      // Assert
      result.IsSuccess.Should().BeTrue();
      result.Value.Id.Should().Be(reader.Id);
      result.Value.WasCreated.Should().BeFalse();
      repository.Verify(r => r.Add(It.IsAny<Reader>()), Times.Never);
      unitOfWork.Verify(
         u => u.SaveAllChangesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
         Times.Never
      );
   }

   [Fact]
   public async Task CreateProvisionAsync_employee_user_fails() {
      // Arrange
      var ct = TestContext.Current.CancellationToken;
      var sut = new ReaderUcCreateProvision(
         identityGateway: new FakeIdentityGateway {
            Subject = Subject,
            Username = Username,
            CreatedAt = CreatedAt,
            IsAuthenticated = true,
            IsReader = false,
            IsEmployee = true
         },
         repository: Mock.Of<IReaderRepository>(),
         unitOfWork: Mock.Of<IUnitOfWork>(),
         logger: Mock.Of<ILogger<ReaderUcCreateProvision>>()
      );

      // Act
      var result = await sut.ExecuteAsync(
         id: null,
         ct: ct
      );

      // Assert
      result.IsFailure.Should().BeTrue();
      result.Error.Should().Be(CommonErrors.AccessNotAllowed);
   }

   [Fact]
   public async Task CreateProvisionAsync_duplicate_email_fails() {
      // Arrange
      var ct = TestContext.Current.CancellationToken;
      var existingReader = Reader.Create(
         id: Guid.Parse("10000000-0000-0000-0000-000000000099"),
         firstname: "Existing",
         lastname: "Reader",
         emailVo: CreateEmail(Username),
         addressVo: CreateAddressVo(),
         subject: "other-subject",
         createdAt: CreatedAt
      ).GetValueOrThrow();

      var repository = new Mock<IReaderRepository>();
      repository
         .Setup(r => r.FindBySubjectAsync(Subject, ct))
         .ReturnsAsync((Reader?)null);
      repository
         .Setup(r => r.FindByEmailAsync(It.IsAny<EmailVo>(), ct))
         .ReturnsAsync(existingReader);

      var sut = new ReaderUcCreateProvision(
         identityGateway: NewReaderIdentity(),
         repository: repository.Object,
         unitOfWork: Mock.Of<IUnitOfWork>(),
         logger: Mock.Of<ILogger<ReaderUcCreateProvision>>()
      );

      // Act
      var result = await sut.ExecuteAsync(
         id: ReaderId,
         ct: ct
      );

      // Assert
      result.IsFailure.Should().BeTrue();
      result.Error.Should().Be(ReaderErrors.EmailAlreadyInUse);
      repository.Verify(r => r.Add(It.IsAny<Reader>()), Times.Never);
   }

   [Fact]
   public async Task UpdateProfileAsync_ok_completes_profile() {
      // Arrange
      var ct = TestContext.Current.CancellationToken;
      var reader = NewProvisionedReader();

      var repository = new Mock<IReaderRepository>();
      repository
         .Setup(r => r.FindBySubjectAsync(Subject, ct))
         .ReturnsAsync(reader);

      var unitOfWork = new Mock<IUnitOfWork>();
      unitOfWork
         .Setup(u => u.SaveAllChangesAsync("ReaderUcUpdateProfile", ct))
         .ReturnsAsync(1);

      var sut = new ReaderUcUpdateProfile(
         identityGateway: NewReaderIdentity(),
         repository: repository.Object,
         unitOfWork: unitOfWork.Object,
         clock: new FakeClock(UpdatedAt),
         logger: Mock.Of<ILogger<ReaderUcUpdateProfile>>()
      );

      var dto = new ReaderProfileUpdateDto(
         Firstname: "Alice",
         Lastname: "Reader",
         AddressDto: CreateAddressDto()
      );

      // Act
      var result = await sut.ExecuteAsync(
         dto: dto,
         ct: ct
      );

      // Assert
      result.IsSuccess.Should().BeTrue();
      result.Value.Firstname.Should().Be("Alice");
      result.Value.Lastname.Should().Be("Reader");
      result.Value.Email.Should().Be(Username);
      result.Value.Subject.Should().Be(Subject);
      result.Value.AddressDto.Should().BeEquivalentTo(dto.AddressDto);
      result.Value.IsProfileCompleted.Should().BeTrue();
      unitOfWork.Verify(
         u => u.SaveAllChangesAsync("ReaderUcUpdateProfile", ct),
         Times.Once
      );
   }

   [Fact]
   public async Task UpdateProfileAsync_not_provisioned_fails() {
      // Arrange
      var ct = TestContext.Current.CancellationToken;
      var repository = new Mock<IReaderRepository>();
      repository
         .Setup(r => r.FindBySubjectAsync(Subject, ct))
         .ReturnsAsync((Reader?)null);

      var sut = new ReaderUcUpdateProfile(
         identityGateway: NewReaderIdentity(),
         repository: repository.Object,
         unitOfWork: Mock.Of<IUnitOfWork>(),
         clock: new FakeClock(UpdatedAt),
         logger: Mock.Of<ILogger<ReaderUcUpdateProfile>>()
      );

      var dto = new ReaderProfileUpdateDto(
         Firstname: "Alice",
         Lastname: "Reader",
         AddressDto: CreateAddressDto()
      );

      // Act
      var result = await sut.ExecuteAsync(
         dto: dto,
         ct: ct
      );

      // Assert
      result.IsFailure.Should().BeTrue();
      result.Error.Should().Be(ReaderErrors.ReaderNotFound);
   }

   [Fact]
   public async Task UpdateProfileAsync_without_address_fails() {
      // Arrange
      var ct = TestContext.Current.CancellationToken;
      var reader = NewProvisionedReader();

      var repository = new Mock<IReaderRepository>();
      repository
         .Setup(r => r.FindBySubjectAsync(Subject, ct))
         .ReturnsAsync(reader);

      var sut = new ReaderUcUpdateProfile(
         identityGateway: NewReaderIdentity(),
         repository: repository.Object,
         unitOfWork: Mock.Of<IUnitOfWork>(),
         clock: new FakeClock(UpdatedAt),
         logger: Mock.Of<ILogger<ReaderUcUpdateProfile>>()
      );

      var dto = new ReaderProfileUpdateDto(
         Firstname: "Alice",
         Lastname: "Reader",
         AddressDto: null!
      );

      // Act
      var result = await sut.ExecuteAsync(
         dto: dto,
         ct: ct
      );

      // Assert
      result.IsFailure.Should().BeTrue();
      result.Error.Should().Be(ReaderErrors.AddressIsRequired);
   }

   private static FakeIdentityGateway NewReaderIdentity() =>
      new() {
         Subject = Subject,
         Username = Username,
         CreatedAt = CreatedAt,
         IsAuthenticated = true,
         IsReader = true,
         IsEmployee = false
      };

   private static Reader NewProvisionedReader() =>
      Reader.Provision(
         id: Guid.Parse(ReaderId),
         subject: Subject,
         emailVo: CreateEmail(Username),
         createdAt: CreatedAt
      ).GetValueOrThrow();

   private static EmailVo CreateEmail(string email) =>
      EmailVo.Create(email).GetValueOrThrow();

   private static AddressVo CreateAddressVo() =>
      AddressVo.Create(
         street: "Profilstraße 1",
         postalCode: "29556",
         city: "Suderburg",
         country: "DE"
      ).GetValueOrThrow();

   private static AddressDto CreateAddressDto() =>
      new(
         Street: "Profilstraße 1",
         PostalCode: "29556",
         City: "Suderburg",
         Country: "DE"
      );
}
