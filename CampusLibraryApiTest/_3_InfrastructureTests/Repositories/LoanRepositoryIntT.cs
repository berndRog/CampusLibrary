using AwesomeAssertions;
using CampusLibraryApi._2_BuildingBlocks._1_Ports;
using CampusLibraryApi._3_Core.Loans._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Loans._3_Domain.Enums;
using CampusLibraryApiTest.TestInfrastructure;
using Microsoft.Extensions.DependencyInjection;
namespace CampusLibraryApiTest._3_InfrastructureTests.Repositories;

public sealed class LoanRepositoryIntT : TestBaseIntegration {
   public LoanRepositoryIntT() {
      DbName = nameof(LoanRepositoryIntT);
      DbMode = DbMode.InMemory;
      SensitiveDataLogging = true;
   }

   [Fact]
   public async Task FindByIdAsync_ok() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var repository = scope.ServiceProvider.GetRequiredService<ILoanRepository>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var loans = seed.Loans;
      var loan1 = loans[0];
      
      repository.AddRange(loans);
      await unitOfWork.SaveAllChangesAsync("Loans inserted", ct);
      unitOfWork.ClearChangeTracker();

      // Act
      var actualLoan = await repository.FindByIdAsync(loan1.Id, ct);

      // Assert
      actualLoan.Should().NotBeNull();

      actualLoan!.Id.Should().Be(loan1.Id);
      actualLoan.ReaderId.Should().Be(loan1.ReaderId);
      actualLoan.BookItemId.Should().Be(loan1.BookItemId);
      actualLoan.LoanDate.Should().Be(loan1.LoanDate);
      actualLoan.DueDate.Should().Be(loan1.DueDate);
      actualLoan.ReturnedAt.Should().BeNull();
      actualLoan.Status.Should().Be(LoanStatus.Active);
      actualLoan.RenewalCount.Should().Be(0);
   }

   [Fact]
   public async Task FindByIdAsync_unknown_id_returns_null() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var repository = scope.ServiceProvider.GetRequiredService<ILoanRepository>();

      // Arrange
      var unknownId = Guid.Parse("99999999-0000-0000-0000-000000000000");

      // Act
      var actualLoan = await repository.FindByIdAsync(unknownId, ct);

      // Assert
      actualLoan.Should().BeNull();
   }

   [Fact]
   public async Task FindActiveByBookItemIdAsync_ok() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var repository = scope.ServiceProvider.GetRequiredService<ILoanRepository>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var loans = seed.Loans;
      var loan1 = loans[0];

      repository.AddRange(loans);
      await unitOfWork.SaveAllChangesAsync("Loans inserted", ct);
      unitOfWork.ClearChangeTracker();
      
      // Act
      var actualLoan = await repository.FindActiveByBookItemIdAsync(
         bookItemId: loan1.BookItemId,
         ct: ct
      );

      // Assert
      actualLoan.Should().NotBeNull();

      actualLoan!.Id.Should().Be(loan1.Id);
      actualLoan.BookItemId.Should().Be(loan1.BookItemId);
      actualLoan.Status.Should().Be(LoanStatus.Active);
   }

   [Fact]
   public async Task FindActiveByBookItemIdAsync_unknown_book_item_id_returns_null() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var repository = scope.ServiceProvider.GetRequiredService<ILoanRepository>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var loans = seed.Loans;
      var unknownBookItemId = Guid.Parse("be999999-0000-0000-0000-000000000000");

      repository.AddRange(loans);
      await unitOfWork.SaveAllChangesAsync("Loans inserted", ct);
      unitOfWork.ClearChangeTracker();

      // Act
      var actualLoan = await repository.FindActiveByBookItemIdAsync(
         bookItemId: unknownBookItemId,
         ct: ct
      );

      // Assert
      actualLoan.Should().BeNull();
   }

   [Fact]
   public async Task FindActiveByBookItemIdAsync_returned_loan_returns_null() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var repository = scope.ServiceProvider.GetRequiredService<ILoanRepository>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var loan1 = seed.Loan1();

      repository.Add(loan1);
      await unitOfWork.SaveAllChangesAsync("Loan inserted", ct);

      var resultReturned = loan1.ReturnAtDesk(
         returnedAt: loan1.LoanDate.AddDays(1)
      );
      resultReturned.IsSuccess.Should().BeTrue();

      await unitOfWork.SaveAllChangesAsync("Loan returned", ct);
      unitOfWork.ClearChangeTracker();

      // Act
      var actualLoan = await repository.FindActiveByBookItemIdAsync(
         loan1.BookItemId, ct);

      // Assert
      actualLoan.Should().BeNull();
   }

   [Fact]
   public async Task FindActiveByReaderIdAsync_ok() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var repository = scope.ServiceProvider.GetRequiredService<ILoanRepository>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var loans = seed.Loans;
      var loan2 = loans[1];

      repository.AddRange(loans);
      await unitOfWork.SaveAllChangesAsync("Loans inserted", ct);
      unitOfWork.ClearChangeTracker();

      var expLoanIds = loans
         .Where(l =>
            l.ReaderId == loan2.ReaderId &&
            l.Status == LoanStatus.Active)
         .Select(l => l.Id)
         .ToList();

      // Act
      var actualLoans = await repository.FindActiveByReaderIdAsync(
         readerId: loan2.ReaderId,
         ct: ct
      );

      // Assert
      actualLoans.Should().NotBeNull();
      actualLoans.Count.Should().Be(expLoanIds.Count);

      var actualLoanIds = actualLoans
         .Select(l => l.Id)
         .ToList();

      actualLoanIds.Should().BeEquivalentTo(
         expLoanIds
      );

      actualLoans.Should().OnlyContain(l =>
         l.ReaderId == loan2.ReaderId &&
         l.Status == LoanStatus.Active);
   }

   [Fact]
   public async Task FindActiveByReaderIdAsync_reader_without_active_loans_returns_empty_list() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var repository = scope.ServiceProvider.GetRequiredService<ILoanRepository>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var loans = seed.Loans;
      var readerWithoutLoansId = Guid.Parse("00000006-0000-0000-0000-000000000000");

      repository.AddRange(loans);
      await unitOfWork.SaveAllChangesAsync("Loans inserted", ct);
      unitOfWork.ClearChangeTracker();

      // Act
      var actualLoans = await repository.FindActiveByReaderIdAsync(
         readerId: readerWithoutLoansId,
         ct: ct
      );

      // Assert
      actualLoans.Should().NotBeNull();
      actualLoans.Should().BeEmpty();
   }

   [Fact]
   public async Task FindActiveByReaderIdAsync_returned_loan_is_not_returned() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var repository = scope.ServiceProvider.GetRequiredService<ILoanRepository>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var loan1 = seed.Loan1();

      repository.Add(loan1);
      await unitOfWork.SaveAllChangesAsync("Loan inserted", ct);

      var resultReturned = loan1.ReturnAtDesk(
         returnedAt: loan1.LoanDate.AddDays(1)
      );
      resultReturned.IsSuccess.Should().BeTrue();
      
      await unitOfWork.SaveAllChangesAsync("Loan returned", ct);
      unitOfWork.ClearChangeTracker();

      // Act
      var actualLoans = await repository.FindActiveByReaderIdAsync(
         readerId: loan1.ReaderId,
         ct: ct
      );

      // Assert
      actualLoans.Should().NotBeNull();
      actualLoans.Should().BeEmpty();
   }

   [Fact]
   public async Task Add_ok() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var repository = scope.ServiceProvider.GetRequiredService<ILoanRepository>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var loan1 = seed.Loan1();

      // Act
      repository.Add(loan1);
      await unitOfWork.SaveAllChangesAsync("Loan inserted", ct);
      unitOfWork.ClearChangeTracker();

      var actualLoan = await repository.FindByIdAsync(loan1.Id, ct);

      // Assert
      actualLoan.Should().NotBeNull();

      actualLoan!.Id.Should().Be(loan1.Id);
      actualLoan.ReaderId.Should().Be(loan1.ReaderId);
      actualLoan.BookItemId.Should().Be(loan1.BookItemId);
      actualLoan.Status.Should().Be(LoanStatus.Active);
   }
}