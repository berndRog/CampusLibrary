using AwesomeAssertions;
using CampusLibraryApi._2_BuildingBlocks._1_Ports;
using CampusLibraryApi._3_Core.Readers._1_Ports;
using CampusLibraryApi._3_Core.Readers._2_Application.Mappings;
using CampusLibraryApiTest.TestInfrastructure;
using Microsoft.Extensions.DependencyInjection;
namespace CampusLibraryApiTest._3_InfrastructureTests.ReadModels;

public sealed class ReaderReadModelIntT : TestBaseIntegration {
   
   public ReaderReadModelIntT() {
      DbName = nameof(ReaderReadModelIntT);
      DbMode = DbMode.InMemory;
      SensitiveDataLogging = true;
   }

   [Fact]
   public async Task FindByIdAsync_ok() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var repository = scope.ServiceProvider.GetRequiredService<IReaderRepository>();
      var readModel = scope.ServiceProvider.GetRequiredService<IReaderReadModel>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var reader1 = seed.Reader1();
      var expReader1Dto = reader1.ToReaderDto();

      repository.Add(reader1);
      await unitOfWork.SaveAllChangesAsync("Reader1 inserted", ct);
      unitOfWork.ClearChangeTracker();

      // Act
      var result = await readModel.FindByIdAsync(reader1.Id, ct);

      // Assert
      result.IsSuccess.Should().BeTrue();
      var actualReader1Dto = result.Value;
      actualReader1Dto.Should().NotBeNull();
      // xUnit uses value equality for records
      actualReader1Dto.Should().BeEquivalentTo(expReader1Dto);
   }

   [Fact]
   public async Task FindByEmailAsync_ok() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var repository = scope.ServiceProvider.GetRequiredService<IReaderRepository>();
      var readModel = scope.ServiceProvider.GetRequiredService<IReaderReadModel>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var reader1 = seed.Reader1();
      var expReader1Dto = reader1.ToReaderDto();

      repository.Add(reader1);
      await unitOfWork.SaveAllChangesAsync("Reader1 inserted", ct);
      unitOfWork.ClearChangeTracker();

      // Act
      var result = await readModel.FindByEmailAsync(reader1.EmailVo.Value, ct);

      // Assert
      result.IsSuccess.Should().BeTrue();
      var actualReader1Dto = result.Value;
      actualReader1Dto.Should().NotBeNull();
      // xUnit uses value equality for records
      actualReader1Dto.Should().BeEquivalentTo(expReader1Dto);
   }
   
   [Fact]
   public async Task SelectAll_ok() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var repository = scope.ServiceProvider.GetRequiredService<IReaderRepository>();
      var readModel = scope.ServiceProvider.GetRequiredService<IReaderReadModel>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var readers = seed.Readers;
      repository.AddRange(readers);
      await unitOfWork.SaveAllChangesAsync("Customers inserted", ct);
      unitOfWork.ClearChangeTracker();
      
      var expCustomerDtos = readers
         .OrderBy(c => c.Id)
         .Select(c => c.ToReaderDto())
         .ToList();

      // Act
      var result = await readModel.SelectAllAsync(ct);

      // Assert
      result.IsSuccess.Should().BeTrue();
      var customerDtos = result.Value
         .OrderBy(c => c.Id)
         .ToList();
      customerDtos.Should().NotBeNull();
      customerDtos.Count.Should().Be(6);
      customerDtos[0].Should().BeEquivalentTo(expCustomerDtos[0]);
      customerDtos.Should().BeEquivalentTo(expCustomerDtos);

      
   }
}