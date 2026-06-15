using AwesomeAssertions;
using CampusLibraryApi._2_BuildingBlocks._1_Ports;
using CampusLibraryApi._3_Core.Readers._1_Ports;
using CampusLibraryApi._3_Core.Readers._2_Application.Mappings;
using CampusLibraryApi._3_Core.Readers._2_Application.UseCases;
using CampusLibraryApi._3_Core.Readers._3_Domain.Entities;
using CampusLibraryApi._3_Core.Readers._3_Domain.Errors;
using CampusLibraryApi._3_Core.Readers._3_Domain.ValueObjects;
using CampusLibraryApiTest.TestHelper.Mappings;
using CampusLibraryApiTest.TestInfrastructure;
using Microsoft.Extensions.Logging;
using Moq;
namespace CampusLibraryApiTest._2_ApplicationTests.UseCases_Mock;

public sealed class ReaderUseCasesMockT {
   private static readonly DateTime CreatedAt =
      new(2025, 01, 01, 00, 00, 00, DateTimeKind.Utc);

   #region ReaderUcCreate
   [Fact]
   public async Task CreateAsync_ok() {
      // Arrange
      var ct = TestContext.Current.CancellationToken;
      var seed = new TestSeed();
      var reader1 = seed.Reader1();
      var dto = Mappings.ToReaderCreateDto(reader1);
      
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

      var sut = new ReaderUcCreate(
         repository: repository.Object,
         unitOfWork: unitOfWork.Object,
         clock: new FakeClock(CreatedAt),
         logger: Mock.Of<ILogger<ReaderUcCreate>>()
      );

      // Act
      var resultCreate = await sut.ExecuteAsync(dto, ct);
      resultCreate.IsSuccess.Should().BeTrue();

      // Assert
      var actualDto = resultCreate.Value;
      actualDto.Id.Should().Be(dto.Id);

      repository.Verify(
         r => r.Add(It.Is<Reader>(reader =>
            reader.Id == reader1.Id &&
            reader.EmailVo.Value == reader1.EmailVo.Value &&
            reader.Subject == reader1.Subject
         )),
         Times.Once
      );

      unitOfWork.Verify(
         u => u.SaveAllChangesAsync("ReaderUcCreate", ct),
         Times.Once
      );
   }

   [Fact]
   public async Task CreateAsync_duplicate_subject_fails() {
      // Arrange
      var ct = TestContext.Current.CancellationToken;
      var seed = new TestSeed();
      var reader = seed.Reader1();
      var dto = Mappings.ToReaderCreateDto(reader); 
      
      var repository = new Mock<IReaderRepository>();
      repository
         .Setup(r => r.ExistsBySubjectAsync(dto.Subject, ct))
         .ReturnsAsync(true);

      var unitOfWork = new Mock<IUnitOfWork>();
      
      var sut = new ReaderUcCreate(
         repository: repository.Object,
         unitOfWork: unitOfWork.Object,
         clock: new FakeClock(CreatedAt),
         logger: Mock.Of<ILogger<ReaderUcCreate>>()
      );

      // Act
      var result = await sut.ExecuteAsync(dto, ct);

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
   public async Task CreateAsync_duplicate_email_fails() {
      // Arrange
      var ct = TestContext.Current.CancellationToken;
      var seed = new TestSeed();
      var reader1 = seed.Reader1();
      var dto = Mappings.ToReaderCreateDto(reader1);
      var reader2 = seed.Reader2();
      var result2 = reader2.UpdateProfile(null, reader1.EmailVo, null, reader1.UpdatedAt);
      result2.IsSuccess.Should().BeTrue();
      
      var repository = new Mock<IReaderRepository>();
      repository
         .Setup(r => r.ExistsBySubjectAsync(dto.Subject, ct))
         .ReturnsAsync(false);
      repository
         .Setup(r => r.FindByEmailAsync(It.IsAny<EmailVo>(), ct))
         .ReturnsAsync(reader2);

      var unitOfWork = new Mock<IUnitOfWork>();
      
      var sut = new ReaderUcCreate(
         repository: repository.Object,
         unitOfWork: unitOfWork.Object,
         clock: new FakeClock(CreatedAt),
         logger: Mock.Of<ILogger<ReaderUcCreate>>()
      );

      // Act
      var result = await sut.ExecuteAsync(dto, ct);

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
   public async Task CreateAsync_invalid_email_fails() {
      // Arrange
      var ct = TestContext.Current.CancellationToken;
      var seed = new TestSeed();
      var reader1 = seed.Reader1();
      var dto = Mappings.ToReaderCreateDto(reader1) with {
         Email = "invalid-email"
      };

      var repository = new Mock<IReaderRepository>();
      repository
         .Setup(r => r.ExistsBySubjectAsync(dto.Subject, ct))
         .ReturnsAsync(false);

      var unitOfWork = new Mock<IUnitOfWork>();
      
      var sut = new ReaderUcCreate(
         repository: repository.Object,
         unitOfWork: unitOfWork.Object,
         clock: new FakeClock(CreatedAt),
         logger: Mock.Of<ILogger<ReaderUcCreate>>()
      );

      // Act
      var result = await sut.ExecuteAsync(dto, ct);

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
   public async Task CreateAsync_invalid_id_fails() {
      // Arrange
      var ct = TestContext.Current.CancellationToken;
      var seed = new TestSeed();
      var reader1 = seed.Reader1();
      var dto = Mappings.ToReaderCreateDto(reader1) with {
         Id = "not-a-guid"
      };
  
      var repository = new Mock<IReaderRepository>();
      repository
         .Setup(r => r.ExistsBySubjectAsync(dto.Subject, ct))
         .ReturnsAsync(false);
      repository
         .Setup(r => r.FindByEmailAsync(It.IsAny<EmailVo>(), ct))
         .ReturnsAsync((Reader?)null);

      var unitOfWork = new Mock<IUnitOfWork>();
      
      var sut = new ReaderUcCreate(
         repository: repository.Object,
         unitOfWork: unitOfWork.Object,
         clock: new FakeClock(CreatedAt),
         logger: Mock.Of<ILogger<ReaderUcCreate>>()
      );

      // Act
      var result = await sut.ExecuteAsync(dto, ct);

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
   #endregion
   
   #region ReaderUcUpdate
      [Fact]
   public async Task UpdateAsync_ok() {
      // Arrange
      var ct = TestContext.Current.CancellationToken;
      var seed = new TestSeed();
      var reader = seed.Reader1();
      var dto = Mappings.ToReaderUpdateDto(reader) with {
         Lastname = "Meier",
         Email = "e.meier@gmx.de",
         AddressDto = seed.Address4Vo.ToAddressDto()
      };
      
      var repository = new Mock<IReaderRepository>();
      repository
         .Setup(r => r.FindByIdAsync(reader.Id, ct))
         .ReturnsAsync(reader);
      repository
         .Setup(r => r.FindByEmailAsync(It.IsAny<EmailVo>(), ct))
         .ReturnsAsync((Reader?)null);

      var unitOfWork = new Mock<IUnitOfWork>();
      
      unitOfWork
         .Setup(u => u.SaveAllChangesAsync("ReaderUcUpdate", ct))
         .ReturnsAsync(1);

      var sut = new ReaderUcUpdate(
         repository: repository.Object,
         unitOfWork: unitOfWork.Object,
         clock: new FakeClock(CreatedAt),
         logger: Mock.Of<ILogger<ReaderUcUpdate>>()
      );

      // Act
      var result = await sut.ExecuteAsync(reader.Id, dto, ct);

      // Assert
      result.IsSuccess.Should().BeTrue();
      var actualDto = result.Value;
      actualDto.Should().BeEquivalentTo(dto);

      unitOfWork.Verify(
         u => u.SaveAllChangesAsync("ReaderUcUpdate", ct),
         Times.Once
      );
   }

   [Fact]
   public async Task UpdateAsync_reader_not_found_fails() {
      // Arrange
      var ct = TestContext.Current.CancellationToken;
      var seed = new TestSeed();
      var reader = seed.Reader1();
      var dto = Mappings.ToReaderUpdateDto(reader) with {
         Lastname = "Meier",
         Email = "e.meier@gmx.de",
         AddressDto = seed.Address4Vo.ToAddressDto()
      };
      
      var repository = new Mock<IReaderRepository>();
      repository
         .Setup(r => r.FindByIdAsync(reader.Id, ct))
         .ReturnsAsync((Reader?)null);

      var unitOfWork = new Mock<IUnitOfWork>();

      var sut = new ReaderUcUpdate(
         repository: repository.Object,
         unitOfWork: unitOfWork.Object,
         clock: new FakeClock(CreatedAt),
         logger: Mock.Of<ILogger<ReaderUcUpdate>>()
      );

      // Act
      var result = await sut.ExecuteAsync(reader.Id, dto, ct);

      // Assert
      result.IsFailure.Should().BeTrue();
      result.Error.Should().Be(ReaderErrors.ReaderNotFound);

      unitOfWork.Verify(
         u => u.SaveAllChangesAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()),
         Times.Never
      );
   }

   [Fact]
   public async Task UpdateAsync_duplicate_email_fails() {
      // Arrange
      var ct = TestContext.Current.CancellationToken;
      var seed = new TestSeed();
      var reader1 = seed.Reader1();
      var reader2 = seed.Reader2();
      var dto = Mappings.ToReaderUpdateDto(reader1) with {
         Lastname = null,
         Email = reader2.EmailVo.Value,
         AddressDto = null
      };
      
      var repository = new Mock<IReaderRepository>();
      repository
         .Setup(r => r.FindByIdAsync(reader1.Id, ct))
         .ReturnsAsync(reader1);
      repository
         .Setup(r => r.FindByEmailAsync(It.IsAny<EmailVo>(), ct))
         .ReturnsAsync(reader2);

      var unitOfWork = new Mock<IUnitOfWork>();
      
      var sut = new ReaderUcUpdate(
         repository: repository.Object,
         unitOfWork: unitOfWork.Object,
         clock: new FakeClock(CreatedAt),
         logger: Mock.Of<ILogger<ReaderUcUpdate>>()
      );
      
      // Act
      var result = await sut.ExecuteAsync(reader1.Id, dto, ct);

      // Assert
      result.IsFailure.Should().BeTrue();
      result.Error.Should().Be(ReaderErrors.EmailAlreadyInUse);

      unitOfWork.Verify(
         u => u.SaveAllChangesAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()),
         Times.Never
      );
   }
   #endregion
   
   #region ReaderUcDelete
   [Fact]
   public async Task DeleteAsync_ok() {
      // Arrange
      var ct = TestContext.Current.CancellationToken;
      var seed = new TestSeed();
      var reader = seed.Reader1();

      var repository = new Mock<IReaderRepository>();
      repository
         .Setup(r => r.FindByIdAsync(reader.Id, ct))
         .ReturnsAsync(reader);

      var unitOfWork = new Mock<IUnitOfWork>();
      unitOfWork
         .Setup(u => u.SaveAllChangesAsync("ReaderUcDelete", ct))
         .ReturnsAsync(1);

      var sut = new ReaderUcDelete(
         repository: repository.Object,
         unitOfWork: unitOfWork.Object,
         logger: Mock.Of<ILogger<ReaderUcDelete>>()
      );
      
      // Act
      var result = await sut.ExecuteAsync(reader.Id, ct);

      // Assert
      result.IsSuccess.Should().BeTrue();

      repository.Verify(
         r => r.Remove(reader),
         Times.Once
      );

      unitOfWork.Verify(
         u => u.SaveAllChangesAsync("ReaderUcDelete", ct),
         Times.Once
      );
   }

   [Fact]
   public async Task DeleteAsync_reader_not_found_fails() {
      // Arrange
      var ct = TestContext.Current.CancellationToken;
      var seed = new TestSeed();
      var reader = seed.Reader1();

      var repository = new Mock<IReaderRepository>();
      repository
         .Setup(r => r.FindByIdAsync(reader.Id, ct))
         .ReturnsAsync((Reader?)null);

      var unitOfWork = new Mock<IUnitOfWork>();

      var sut = new ReaderUcDelete(
         repository: repository.Object,
         unitOfWork: unitOfWork.Object,
         logger: Mock.Of<ILogger<ReaderUcDelete>>()
      );

      // Act
      var result = await sut.ExecuteAsync(reader.Id, ct);

      // Assert
      result.IsFailure.Should().BeTrue();
      result.Error.Should().Be(ReaderErrors.ReaderNotFound);

      repository.Verify(
         r => r.Remove(It.IsAny<Reader>()),
         Times.Never
      );

      unitOfWork.Verify(
         u => u.SaveAllChangesAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()),
         Times.Never
      );
   }

   [Fact]
   public async Task DeleteAsync_empty_id_fails() {
      // Arrange
      var ct = TestContext.Current.CancellationToken;
      var repository = new Mock<IReaderRepository>();
      
      var unitOfWork = new Mock<IUnitOfWork>();
      
      var sut = new ReaderUcDelete(
         repository: repository.Object,
         unitOfWork: unitOfWork.Object,
         logger: Mock.Of<ILogger<ReaderUcDelete>>()
      );

      // Act
      var result = await sut.ExecuteAsync(Guid.Empty, ct);

      // Assert
      result.IsFailure.Should().BeTrue();
      result.Error.Should().Be(ReaderErrors.InvalidId);

      repository.Verify(
         r => r.FindByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
         Times.Never
      );

      repository.Verify(
         r => r.Remove(It.IsAny<Reader>()),
         Times.Never
      );

      unitOfWork.Verify(
         u => u.SaveAllChangesAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()),
         Times.Never
      );
   }
   #endregion
}
