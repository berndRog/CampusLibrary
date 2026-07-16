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
      new(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
   private static readonly DateTime UpdatedAt =
      new(2025, 1, 2, 0, 0, 0, DateTimeKind.Utc);

   private const string Subject = "identity-subject-001";
   private const string Username = "reader.one@example.org";
   private const string ReaderId = "10000000-0000-0000-0000-000000000001";

   [Fact]
   public async Task Provision_creates_incomplete_reader_and_reports_created() {
      var ct = TestContext.Current.CancellationToken;
      var repository = new Mock<IReaderRepository>();
      repository.Setup(r => r.FindBySubjectAsync(Subject, ct)).ReturnsAsync((Reader?)null);
      repository.Setup(r => r.FindByEmailAsync(It.IsAny<EmailVo>(), ct)).ReturnsAsync((Reader?)null);

      var uow = new Mock<IUnitOfWork>();
      uow.Setup(u => u.SaveAllChangesAsync("ReaderUcCreateMeProvision", ct)).ReturnsAsync(1);

      var sut = new ReaderUcCreateMeProvision(
         NewReaderIdentity(), repository.Object, uow.Object,
         Mock.Of<ILogger<ReaderUcCreateMeProvision>>()
      );

      var result = await sut.ExecuteAsync(ReaderId, ct);

      result.IsSuccess.Should().BeTrue();
      result.Value.Should().BeTrue();
      repository.Verify(r => r.Add(It.Is<Reader>(reader =>
         reader.Id == Guid.Parse(ReaderId) &&
         reader.Subject == Subject &&
         reader.EmailVo.Value == Username &&
         !reader.IsProfileCompleted
      )), Times.Once);
   }

   [Fact]
   public async Task Provision_existing_subject_is_idempotent() {
      var ct = TestContext.Current.CancellationToken;
      var repository = new Mock<IReaderRepository>();
      repository.Setup(r => r.FindBySubjectAsync(Subject, ct)).ReturnsAsync(NewProvisionedReader());

      var sut = new ReaderUcCreateMeProvision(
         NewReaderIdentity(), repository.Object, Mock.Of<IUnitOfWork>(),
         Mock.Of<ILogger<ReaderUcCreateMeProvision>>()
      );

      var result = await sut.ExecuteAsync(null, ct);

      result.IsSuccess.Should().BeTrue();
      result.Value.Should().BeFalse();
      repository.Verify(r => r.Add(It.IsAny<Reader>()), Times.Never);
   }

   [Fact]
   public async Task Provision_employee_user_fails() {
      var ct = TestContext.Current.CancellationToken;
      var identity = new FakeIdentityGateway {
         Subject = Subject,
         Username = Username,
         CreatedAt = CreatedAt,
         IsAuthenticated = true,
         IsReader = false,
         IsEmployee = true
      };

      var sut = new ReaderUcCreateMeProvision(
         identity, Mock.Of<IReaderRepository>(), Mock.Of<IUnitOfWork>(),
         Mock.Of<ILogger<ReaderUcCreateMeProvision>>()
      );

      var result = await sut.ExecuteAsync(null, ct);

      result.IsFailure.Should().BeTrue();
      result.Error.Should().Be(CommonErrors.AccessNotAllowed);
   }

   [Fact]
   public async Task Provision_duplicate_email_fails() {
      var ct = TestContext.Current.CancellationToken;
      var existingReader = Reader.Create(
         Guid.NewGuid(), "Other", "Reader", CreateEmail(Username),
         CreateAddressVo(), "other-subject", CreatedAt
      ).GetValueOrThrow();

      var repository = new Mock<IReaderRepository>();
      repository.Setup(r => r.FindBySubjectAsync(Subject, ct)).ReturnsAsync((Reader?)null);
      repository.Setup(r => r.FindByEmailAsync(It.IsAny<EmailVo>(), ct)).ReturnsAsync(existingReader);

      var sut = new ReaderUcCreateMeProvision(
         NewReaderIdentity(), repository.Object, Mock.Of<IUnitOfWork>(),
         Mock.Of<ILogger<ReaderUcCreateMeProvision>>()
      );

      var result = await sut.ExecuteAsync(ReaderId, ct);

      result.IsFailure.Should().BeTrue();
      result.Error.Should().Be(ReaderErrors.EmailAlreadyInUse);
      repository.Verify(r => r.Add(It.IsAny<Reader>()), Times.Never);
   }

   [Fact]
   public async Task Profile_completes_initial_profile_and_keeps_provisioned_email() {
      var ct = TestContext.Current.CancellationToken;
      var reader = NewProvisionedReader();
      var repository = new Mock<IReaderRepository>();
      repository.Setup(r => r.FindBySubjectAsync(Subject, ct)).ReturnsAsync(reader);

      var uow = new Mock<IUnitOfWork>();
      uow.Setup(u => u.SaveAllChangesAsync("ReaderUcUpdateMeProfile", ct)).ReturnsAsync(1);

      var dto = NewProfileDto("Alice", "Reader", SecondAddress());
      var result = await NewProfileSut(repository.Object, uow.Object).ExecuteAsync(dto, ct);

      result.IsSuccess.Should().BeTrue();
      result.Value.Firstname.Should().Be(dto.Firstname);
      result.Value.Lastname.Should().Be(dto.Lastname);
      result.Value.Email.Should().Be(Username);
      result.Value.AddressDto.Should().BeEquivalentTo(dto.AddressDto);
      result.Value.IsProfileCompleted.Should().BeTrue();
   }

   [Fact]
   public async Task Profile_not_provisioned_fails() {
      var ct = TestContext.Current.CancellationToken;
      var repository = new Mock<IReaderRepository>();
      repository.Setup(r => r.FindBySubjectAsync(Subject, ct)).ReturnsAsync((Reader?)null);

      var result = await NewProfileSut(repository.Object, Mock.Of<IUnitOfWork>())
         .ExecuteAsync(NewProfileDto(), ct);

      result.IsFailure.Should().BeTrue();
      result.Error.Should().Be(ReaderErrors.ReaderNotFound);
   }

   [Fact]
   public async Task Profile_without_address_fails() {
      var ct = TestContext.Current.CancellationToken;
      var reader = NewProvisionedReader();
      var repository = new Mock<IReaderRepository>();
      repository.Setup(r => r.FindBySubjectAsync(Subject, ct)).ReturnsAsync(reader);

      var dto = new ReaderProfileDto(
         Firstname: "Alice",
         Lastname: "Reader",
         AddressDto: null!
      );

      var result = await NewProfileSut(repository.Object, Mock.Of<IUnitOfWork>())
         .ExecuteAsync(dto, ct);

      result.IsFailure.Should().BeTrue();
      result.Error.Should().Be(ReaderErrors.AddressIsRequired);
   }

   [Fact]
   public async Task Update_me_changes_only_mutable_self_service_data() {
      var ct = TestContext.Current.CancellationToken;
      var reader = NewCompletedReader();
      var repository = new Mock<IReaderRepository>();
      repository.Setup(r => r.FindBySubjectAsync(Subject, ct)).ReturnsAsync(reader);
      repository.Setup(r => r.FindByEmailAsync(It.IsAny<EmailVo>(), ct)).ReturnsAsync((Reader?)null);

      var uow = new Mock<IUnitOfWork>();
      uow.Setup(u => u.SaveAllChangesAsync("ReaderUcUpdateMe", ct)).ReturnsAsync(1);

      var dto = new ReaderUpdateDto(
         Lastname: "Changed",
         Email: "reader.changed@example.org",
         AddressDto: SecondAddress()
      );

      var result = await NewUpdateSut(repository.Object, uow.Object).ExecuteAsync(dto, ct);

      result.IsSuccess.Should().BeTrue();
      result.Value.Firstname.Should().Be("Alice");
      result.Value.Lastname.Should().Be("Changed");
      result.Value.Email.Should().Be("reader.changed@example.org");
      result.Value.AddressDto.Should().BeEquivalentTo(SecondAddress());
      uow.Verify(u => u.SaveAllChangesAsync("ReaderUcUpdateMe", ct), Times.Once);
   }

   [Fact]
   public async Task Update_me_not_provisioned_fails() {
      var ct = TestContext.Current.CancellationToken;
      var repository = new Mock<IReaderRepository>();
      repository.Setup(r => r.FindBySubjectAsync(Subject, ct)).ReturnsAsync((Reader?)null);

      var result = await NewUpdateSut(repository.Object, Mock.Of<IUnitOfWork>())
         .ExecuteAsync(new ReaderUpdateDto("Changed", null, null), ct);

      result.IsFailure.Should().BeTrue();
      result.Error.Should().Be(ReaderErrors.ReaderNotFound);
   }

   [Fact]
   public async Task Update_me_duplicate_email_fails() {
      var ct = TestContext.Current.CancellationToken;
      var reader = NewCompletedReader();
      var otherReader = Reader.Create(
         Guid.NewGuid(), "Other", "Reader", CreateEmail("other@example.org"),
         CreateAddressVo(), "other-subject", CreatedAt
      ).GetValueOrThrow();

      var repository = new Mock<IReaderRepository>();
      repository.Setup(r => r.FindBySubjectAsync(Subject, ct)).ReturnsAsync(reader);
      repository.Setup(r => r.FindByEmailAsync(It.IsAny<EmailVo>(), ct)).ReturnsAsync(otherReader);

      var result = await NewUpdateSut(repository.Object, Mock.Of<IUnitOfWork>())
         .ExecuteAsync(new ReaderUpdateDto(null, otherReader.EmailVo.Value, null), ct);

      result.IsFailure.Should().BeTrue();
      result.Error.Should().Be(ReaderErrors.EmailAlreadyInUse);
   }

   private static ReaderUcUpdateMeProfile NewProfileSut(
      IReaderRepository repository,
      IUnitOfWork uow
   ) => new(
      NewReaderIdentity(), repository, uow, new FakeClock(UpdatedAt),
      Mock.Of<ILogger<ReaderUcUpdateMeProfile>>()
   );

   private static ReaderUcUpdateMe NewUpdateSut(
      IReaderRepository repository,
      IUnitOfWork uow
   ) => new(
      NewReaderIdentity(), repository, uow, new FakeClock(UpdatedAt),
      Mock.Of<ILogger<ReaderUcUpdateMe>>()
   );

   private static ReaderProfileDto NewProfileDto(
      string firstname = "Alice",
      string lastname = "Reader",
      AddressDto? address = null
   ) => new(firstname, lastname, address ?? FirstAddress());

   private static FakeIdentityGateway NewReaderIdentity() => new() {
      Subject = Subject,
      Username = Username,
      CreatedAt = CreatedAt,
      IsAuthenticated = true,
      IsReader = true,
      IsEmployee = false
   };

   private static Reader NewProvisionedReader() => Reader.Provision(
      Guid.Parse(ReaderId), Subject, CreateEmail(Username), CreatedAt
   ).GetValueOrThrow();

   private static Reader NewCompletedReader() {
      var reader = NewProvisionedReader();
      reader.UpdateMyProfile(
         "Alice", "Reader", CreateAddressVo(), UpdatedAt
      ).IsSuccess.Should().BeTrue();
      return reader;
   }

   private static EmailVo CreateEmail(string email) => EmailVo.Create(email).GetValueOrThrow();

   private static AddressVo CreateAddressVo() => AddressVo.Create(
      "Profilstraße 1", "29556", "Suderburg", "DE"
   ).GetValueOrThrow();

   private static AddressDto FirstAddress() =>
      new("Profilstraße 1", "29556", "Suderburg", "DE");

   private static AddressDto SecondAddress() =>
      new("Neue Straße 7", "29556", "Suderburg", "DE");
}
