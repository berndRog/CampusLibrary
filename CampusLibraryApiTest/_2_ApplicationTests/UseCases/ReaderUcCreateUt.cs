using AwesomeAssertions;
using CampusLibraryApi._2_Shared._1_Ports;
using CampusLibraryApi._3_Core.Readers._1_Ports;
using CampusLibraryApi._3_Core.Readers._2_Application.Dtos;
using CampusLibraryApi._3_Core.Readers._2_Application.UseCases;
using CampusLibraryApi._3_Core.Readers._3_Domain.Entities;
using CampusLibraryApi._3_Core.Readers._3_Domain.Errors;
using CampusLibraryApi._3_Core.Readers._3_Domain.ValueObjects;
using CampusLibraryApiTest.TestInfrastructure;
using Microsoft.Extensions.Logging;
using Moq;

namespace CampusLibraryApiTest._2_ApplicationTests.UseCases;

public sealed class ReaderUcCreateUt {
   private static readonly DateTime CreatedAt =
      new(2025, 01, 01, 00, 00, 00, DateTimeKind.Utc);

   [Fact]
   public async Task ExecuteAsync_ok() {
      // Arrange
      var ct = TestContext.Current.CancellationToken;
      var dto = CreateDto(
         id: "10000000-0000-0000-0000-000000000000",
         firstname: "Erika",
         lastname: "Mustermann",
         email: "ERIKA.MUSTERMANN@EXAMPLE.COM",
         subject: "subject-001"
      );

      var repository = new Mock<IReaderRepository>();
      repository
         .Setup(r => r.ExistsBySubjectAsync(dto.Subject, ct))
         .ReturnsAsync(false);
      repository
         .Setup(r => r.FindByEmailAsync(It.IsAny<EmailVo>(), ct))
         .ReturnsAsync((Reader?)null);

      var unitOfWork = new Mock<IUnitOfWork>();
      unitOfWork
         .Setup(u => u.SaveAllChangesAsync("ReaderUcCreate", ct))
         .ReturnsAsync(1);

      var uc = CreateUseCase(repository, unitOfWork);

      // Act
      var result = await uc.ExecuteAsync(dto, ct);

      // Assert
      result.IsSuccess.Should().BeTrue();
      result.Value.Id.Should().Be(Guid.Parse(dto.Id!));
      result.Value.Firstname.Should().Be("Erika");
      result.Value.Lastname.Should().Be("Mustermann");
      result.Value.Email.Should().Be("erika.mustermann@example.com");
      result.Value.Subject.Should().Be("subject-001");

      repository.Verify(
         r => r.Add(It.Is<Reader>(reader =>
            reader.Id == Guid.Parse(dto.Id!) &&
            reader.EmailVo.Value == "erika.mustermann@example.com" &&
            reader.Subject == "subject-001"
         )),
         Times.Once
      );

      unitOfWork.Verify(
         u => u.SaveAllChangesAsync("ReaderUcCreate", ct),
         Times.Once
      );
   }

   [Fact]
   public async Task ExecuteAsync_duplicate_subject_fails() {
      // Arrange
      var ct = TestContext.Current.CancellationToken;
      var dto = CreateDto(subject: "subject-001");

      var repository = new Mock<IReaderRepository>();
      repository
         .Setup(r => r.ExistsBySubjectAsync(dto.Subject, ct))
         .ReturnsAsync(true);

      var unitOfWork = new Mock<IUnitOfWork>();
      var uc = CreateUseCase(repository, unitOfWork);

      // Act
      var result = await uc.ExecuteAsync(dto, ct);

      // Assert
      result.IsFailure.Should().BeTrue();
      result.Error.Should().Be(ReaderErrors.SubjectAlreadyExists);

      repository.Verify(
         r => r.Add(It.IsAny<Reader>()),
         Times.Never
      );
      unitOfWork.Verify(
         u => u.SaveAllChangesAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()),
         Times.Never
      );
   }

   [Fact]
   public async Task ExecuteAsync_duplicate_email_fails() {
      // Arrange
      var ct = TestContext.Current.CancellationToken;
      var existingReader = CreateReader(
         id: Guid.Parse("20000000-0000-0000-0000-000000000000"),
         email: "erika.mustermann@example.com",
         subject: "subject-002"
      );
      var dto = CreateDto(
         email: "ERIKA.MUSTERMANN@EXAMPLE.COM",
         subject: "subject-001"
      );

      var repository = new Mock<IReaderRepository>();
      repository
         .Setup(r => r.ExistsBySubjectAsync(dto.Subject, ct))
         .ReturnsAsync(false);
      repository
         .Setup(r => r.FindByEmailAsync(It.IsAny<EmailVo>(), ct))
         .ReturnsAsync(existingReader);

      var unitOfWork = new Mock<IUnitOfWork>();
      var uc = CreateUseCase(repository, unitOfWork);

      // Act
      var result = await uc.ExecuteAsync(dto, ct);

      // Assert
      result.IsFailure.Should().BeTrue();
      result.Error.Should().Be(ReaderErrors.EmailAlreadyInUse);

      repository.Verify(
         r => r.Add(It.IsAny<Reader>()),
         Times.Never
      );
      unitOfWork.Verify(
         u => u.SaveAllChangesAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()),
         Times.Never
      );
   }

   [Fact]
   public async Task ExecuteAsync_invalid_email_fails() {
      // Arrange
      var ct = TestContext.Current.CancellationToken;
      var dto = CreateDto(email: "invalid-email", subject: "subject-001");

      var repository = new Mock<IReaderRepository>();
      repository
         .Setup(r => r.ExistsBySubjectAsync(dto.Subject, ct))
         .ReturnsAsync(false);

      var unitOfWork = new Mock<IUnitOfWork>();
      var uc = CreateUseCase(repository, unitOfWork);

      // Act
      var result = await uc.ExecuteAsync(dto, ct);

      // Assert
      result.IsFailure.Should().BeTrue();
      result.Error.Should().Be(ReaderErrors.InvalidEmail);

      repository.Verify(
         r => r.Add(It.IsAny<Reader>()),
         Times.Never
      );
      unitOfWork.Verify(
         u => u.SaveAllChangesAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()),
         Times.Never
      );
   }

   [Fact]
   public async Task ExecuteAsync_invalid_id_fails() {
      // Arrange
      var ct = TestContext.Current.CancellationToken;
      var dto = CreateDto(id: "not-a-guid", subject: "subject-001");

      var repository = new Mock<IReaderRepository>();
      repository
         .Setup(r => r.ExistsBySubjectAsync(dto.Subject, ct))
         .ReturnsAsync(false);
      repository
         .Setup(r => r.FindByEmailAsync(It.IsAny<EmailVo>(), ct))
         .ReturnsAsync((Reader?)null);

      var unitOfWork = new Mock<IUnitOfWork>();
      var uc = CreateUseCase(repository, unitOfWork);

      // Act
      var result = await uc.ExecuteAsync(dto, ct);

      // Assert
      result.IsFailure.Should().BeTrue();
      result.Error.Should().Be(ReaderErrors.InvalidId);

      repository.Verify(
         r => r.Add(It.IsAny<Reader>()),
         Times.Never
      );
      unitOfWork.Verify(
         u => u.SaveAllChangesAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()),
         Times.Never
      );
   }

   private static ReaderUcCreate CreateUseCase(
      Mock<IReaderRepository> repository,
      Mock<IUnitOfWork> unitOfWork
   ) => new(
      repository: repository.Object,
      unitOfWork: unitOfWork.Object,
      clock: new FakeClock(CreatedAt),
      logger: Mock.Of<ILogger<ReaderUcCreate>>()
   );

   private static ReaderCreateDto CreateDto(
      string? id = "10000000-0000-0000-0000-000000000000",
      string firstname = "Erika",
      string lastname = "Mustermann",
      string email = "erika.mustermann@example.com",
      string subject = "subject-001"
   ) => new(
      Firstname: firstname,
      Lastname: lastname,
      Email: email,
      AddressDto: new AddressDto(
         Street: "Hauptstr. 23",
         PostalCode: "29556",
         City: "Suderburg",
         Country: "DE"
      ),
      Subject: subject,
      Id: id
   );

   private static Reader CreateReader(
      Guid id,
      string email,
      string subject
   ) {
      var emailVo = EmailVo.Create(email).GetValueOrThrow();
      var addressVo = AddressVo.Create(
         street: "Hauptstr. 23",
         postalCode: "29556",
         city: "Suderburg",
         country: "DE"
      ).GetValueOrThrow();

      return Reader.Create(
         id: id,
         firstname: "Erika",
         lastname: "Mustermann",
         emailVo: emailVo,
         addressVo: addressVo,
         subject: subject,
         createdAt: CreatedAt
      ).GetValueOrThrow();
   }
}
