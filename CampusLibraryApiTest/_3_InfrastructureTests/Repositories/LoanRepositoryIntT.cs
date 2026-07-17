using AwesomeAssertions;
using CampusLibraryApi._2_BuildingBlocks._1_Ports;
using CampusLibraryApi._3_Core.Loans._1_Ports.Outbound;
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

      repository.AddRange(
         loans: loans
      );

      await unitOfWork.SaveAllChangesAsync(
         "Loans inserted",
         ct
      );

      unitOfWork.ClearChangeTracker();

      // Act
      var actualLoan = await repository.FindByIdAsync(
         id: loan1.Id,
         ct: ct
      );

      // Assert
      actualLoan.Should().NotBeNull();
      actualLoan!.Id.Should().Be(loan1.Id);
      actualLoan.ReaderId.Should().Be(loan1.ReaderId);
      actualLoan.BookItemId.Should().Be(loan1.BookItemId);
      actualLoan.LoanDate.Should().Be(loan1.LoanDate);
      actualLoan.DueDate.Should().Be(loan1.DueDate);
      actualLoan.RenewalCount.Should().Be(loan1.RenewalCount);
   }

   [Fact]
   public async Task FindByIdAsync_unknown_id_returns_null() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var repository = scope.ServiceProvider.GetRequiredService<ILoanRepository>();

      // Arrange
      var unknownId = Guid.Parse("99999999-0000-0000-0000-000000000000");

      // Act
      var actualLoan = await repository.FindByIdAsync(
         id: unknownId,
         ct: ct
      );

      // Assert
      actualLoan.Should().BeNull();
   }

   [Fact]
   public async Task FindBorrowedByBookItemIdAsync_ok() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var repository = scope.ServiceProvider.GetRequiredService<ILoanRepository>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var loans = seed.Loans;
      var loan1 = loans[0];

      repository.AddRange(
         loans: loans
      );

      await unitOfWork.SaveAllChangesAsync(
         "Loans inserted",
         ct
      );

      unitOfWork.ClearChangeTracker();

      // Act
      var actualLoan = await repository.FindBorrowedByBookItemIdAsync(
         bookItemId: loan1.BookItemId,
         ct: ct
      );

      // Assert
      actualLoan.Should().NotBeNull();
      actualLoan!.Id.Should().Be(loan1.Id);
      actualLoan.BookItemId.Should().Be(loan1.BookItemId);
   }

   [Fact]
   public async Task FindBorrowedByBookItemIdAsync_unknown_book_item_id_returns_null() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var repository = scope.ServiceProvider.GetRequiredService<ILoanRepository>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var loans = seed.Loans;
      var unknownBookItemId = Guid.Parse("be999999-0000-0000-0000-000000000000");

      repository.AddRange(
         loans: loans
      );

      await unitOfWork.SaveAllChangesAsync(
         "Loans inserted",
         ct
      );

      unitOfWork.ClearChangeTracker();

      // Act
      var actualLoan = await repository.FindBorrowedByBookItemIdAsync(
         bookItemId: unknownBookItemId,
         ct: ct
      );

      // Assert
      actualLoan.Should().BeNull();
   }

   [Fact]
   public async Task FindBorrowedByReaderIdAsync_ok() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var repository = scope.ServiceProvider.GetRequiredService<ILoanRepository>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var loans = seed.Loans;
      var loan2 = loans[1];

      repository.AddRange(
         loans: loans
      );

      await unitOfWork.SaveAllChangesAsync(
         "Loans inserted",
         ct
      );

      unitOfWork.ClearChangeTracker();

      var expectedLoanIds = loans
         .Where(l => l.ReaderId == loan2.ReaderId)
         .OrderBy(l => l.DueDate)
         .ThenBy(l => l.LoanDate)
         .ThenBy(l => l.Id)
         .Select(l => l.Id)
         .ToList();

      // Act
      var actualLoans = await repository.FindBorrowedByReaderIdAsync(
         readerId: loan2.ReaderId,
         ct: ct
      );

      // Assert
      actualLoans.Should().NotBeNull();

      actualLoans
         .Select(l => l.Id)
         .Should()
         .BeEquivalentTo(
            expectedLoanIds,
            options => options.WithStrictOrdering()
         );

      actualLoans.Should().OnlyContain(l =>
         l.ReaderId == loan2.ReaderId
      );
   }

   [Fact]
   public async Task FindBorrowedByReaderIdAsync_reader_without_borrowed_loans_returns_empty_list() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var repository = scope.ServiceProvider.GetRequiredService<ILoanRepository>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var loans = seed.Loans;
      var readerWithoutBorrowedLoansId = Guid.Parse("00000006-0000-0000-0000-000000000000");

      repository.AddRange(
         loans: loans
      );

      await unitOfWork.SaveAllChangesAsync(
         "Loans inserted",
         ct
      );

      unitOfWork.ClearChangeTracker();

      // Act
      var actualLoans = await repository.FindBorrowedByReaderIdAsync(
         readerId: readerWithoutBorrowedLoansId,
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
      repository.Add(
         loan: loan1
      );

      await unitOfWork.SaveAllChangesAsync(
         "Loan inserted",
         ct
      );

      unitOfWork.ClearChangeTracker();

      var actualLoan = await repository.FindByIdAsync(
         id: loan1.Id,
         ct: ct
      );

      // Assert
      actualLoan.Should().NotBeNull();
      actualLoan!.Id.Should().Be(loan1.Id);
   }

   [Fact]
   public async Task Remove_ok_deletes_loan() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var repository = scope.ServiceProvider.GetRequiredService<ILoanRepository>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var loan1 = seed.Loan1();

      repository.Add(
         loan: loan1
      );

      await unitOfWork.SaveAllChangesAsync(
         "Loan inserted",
         ct
      );

      unitOfWork.ClearChangeTracker();

      var persistedLoan = await repository.FindByIdAsync(
         id: loan1.Id,
         ct: ct
      );

      persistedLoan.Should().NotBeNull();

      // Act
      repository.Remove(
         loan: persistedLoan!
      );

      await unitOfWork.SaveAllChangesAsync(
         "Loan removed",
         ct
      );

      unitOfWork.ClearChangeTracker();

      var deletedLoan = await repository.FindByIdAsync(
         id: loan1.Id,
         ct: ct
      );

      // Assert
      deletedLoan.Should().BeNull();
   }

}
