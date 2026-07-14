using AwesomeAssertions;
using CampusLibraryApi._2_BuildingBlocks._1_Ports;
using CampusLibraryApi._3_Core.Loans._1_Ports.Outbound;
using CampusLibraryApiTest.TestInfrastructure;
using Microsoft.Extensions.DependencyInjection;

using CampusLibraryApi._2_BuildingBlocks._1_Ports.Contracts;
namespace CampusLibraryApiTest._3_InfrastructureTests.Contracts;

public sealed class LoanReaderContractIntT : TestBaseIntegration {
   public LoanReaderContractIntT() {
      DbName = nameof(LoanReaderContractIntT);
      DbMode = DbMode.InMemory;
      SensitiveDataLogging = true;
   }

   [Fact]
   public async Task ExistsForReaderAsync_without_loan_returns_false() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var contract = scope.ServiceProvider.GetRequiredService<ILoanReaderContract>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      bool exists = await contract.ExistsForReaderAsync(
         readerId: seed.Reader1().Id,
         ct: ct
      );

      exists.Should().BeFalse();
   }

   [Fact]
   public async Task ExistsForReaderAsync_with_current_loan_returns_true() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var contract = scope.ServiceProvider.GetRequiredService<ILoanReaderContract>();
      var loanRepository = scope.ServiceProvider.GetRequiredService<ILoanRepository>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      var loan = seed.Loan1();
      loanRepository.Add(loan);

      await unitOfWork.SaveAllChangesAsync(
         "Loan inserted",
         ct
      );
      unitOfWork.ClearChangeTracker();

      bool exists = await contract.ExistsForReaderAsync(
         readerId: loan.ReaderId,
         ct: ct
      );

      exists.Should().BeTrue();
   }
}
