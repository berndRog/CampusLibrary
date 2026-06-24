using AwesomeAssertions;
using CampusLibraryApi._2_BuildingBlocks._1_Ports;
using CampusLibraryApi._3_Core.Loans._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Loans._3_Domain.Entities;
using CampusLibraryApi._3_Core.Loans._3_Domain.Enums;
using CampusLibraryApi._3_Core.Loans._3_Domain.Errors;
using CampusLibraryApiTest.TestInfrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace CampusLibraryApiTest._3_InfrastructureTests.ReadModels;

public sealed class LoanReadModelIntT : TestBaseIntegration {
   public LoanReadModelIntT() {
      DbName = nameof(LoanReadModelIntT);
      DbMode = DbMode.InMemory;
      SensitiveDataLogging = true;
   }

   [Fact]
   public async Task FindByIdAsync_ok() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var loanRepository = scope.ServiceProvider.GetRequiredService<ILoanRepository>();
      var readModel = scope.ServiceProvider.GetRequiredService<ILoanReadModel>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var loans = seed.Loans;
      var loan1 = loans[0];

      AddLoans(
         loanRepository: loanRepository,
         loans: loans
      );

      await unitOfWork.SaveAllChangesAsync(
         "Loans inserted",
         ct
      );

      unitOfWork.ClearChangeTracker();

      // Act
      var result = await readModel.FindByIdAsync(
         id: loan1.Id,
         ct: ct
      );

      // Assert
      result.IsSuccess.Should().BeTrue();

      var actualLoanDto = result.Value;

      actualLoanDto.Should().NotBeNull();
      actualLoanDto.Id.Should().Be(loan1.Id);
      actualLoanDto.ReaderId.Should().Be(loan1.ReaderId);
      actualLoanDto.BookItemId.Should().Be(loan1.BookItemId);
      actualLoanDto.LoanDate.Should().Be(loan1.LoanDate);
      actualLoanDto.DueDate.Should().Be(loan1.DueDate);
      actualLoanDto.ReturnedAt.Should().Be(loan1.ReturnedAt);
      actualLoanDto.Status.Should().Be(loan1.Status);
      actualLoanDto.RenewalCount.Should().Be(loan1.RenewalCount);
   }

   [Fact]
   public async Task FindByIdAsync_empty_id_returns_failure() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var readModel = scope.ServiceProvider.GetRequiredService<ILoanReadModel>();

      // Act
      var result = await readModel.FindByIdAsync(
         id: Guid.Empty,
         ct: ct
      );

      // Assert
      result.IsFailure.Should().BeTrue();
      result.Error.Should().Be(LoanErrors.LoanIdRequired);
   }

   [Fact]
   public async Task FindByIdAsync_unknown_id_returns_failure() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var readModel = scope.ServiceProvider.GetRequiredService<ILoanReadModel>();

      // Arrange
      var unknownId = Guid.Parse("99999999-0000-0000-0000-000000000000");

      // Act
      var result = await readModel.FindByIdAsync(
         id: unknownId,
         ct: ct
      );

      // Assert
      result.IsFailure.Should().BeTrue();
      result.Error.Should().Be(LoanErrors.LoanNotFound);
   }

   [Fact]
   public async Task FindAllActiveAsync_ok() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var loanRepository = scope.ServiceProvider.GetRequiredService<ILoanRepository>();
      var readModel = scope.ServiceProvider.GetRequiredService<ILoanReadModel>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var loans = seed.Loans;

      AddLoans(
         loanRepository: loanRepository,
         loans: loans
      );

      await unitOfWork.SaveAllChangesAsync(
         "Loans inserted",
         ct
      );

      unitOfWork.ClearChangeTracker();

      var expLoanIds = loans
         .Where(l => l.Status == LoanStatus.Active)
         .OrderBy(l => l.DueDate)
         .Select(l => l.Id)
         .ToList();

      // Act
      var result = await readModel.FindAllActiveAsync(
         ct: ct
      );

      // Assert
      result.IsSuccess.Should().BeTrue();

      var actualLoanDtos = result.Value;

      actualLoanDtos.Should().NotBeNull();
      actualLoanDtos.Count.Should().Be(expLoanIds.Count);

      var actualLoanIds = actualLoanDtos
         .Select(l => l.Id)
         .ToList();

      actualLoanIds.Should().BeEquivalentTo(
         expLoanIds,
         options => options.WithStrictOrdering()
      );

      actualLoanDtos.Should().OnlyContain(l =>
         l.Status == LoanStatus.Active);
   }

   [Fact]
   public async Task FindActiveByReaderIdAsync_ok() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var loanRepository = scope.ServiceProvider.GetRequiredService<ILoanRepository>();
      var readModel = scope.ServiceProvider.GetRequiredService<ILoanReadModel>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var loans = seed.Loans;
      var loan2 = loans[1];

      AddLoans(
         loanRepository: loanRepository,
         loans: loans
      );

      await unitOfWork.SaveAllChangesAsync(
         "Loans inserted",
         ct
      );

      unitOfWork.ClearChangeTracker();

      var expLoanIds = loans
         .Where(l =>
            l.ReaderId == loan2.ReaderId &&
            l.Status == LoanStatus.Active)
         .OrderBy(l => l.DueDate)
         .Select(l => l.Id)
         .ToList();

      // Act
      var result = await readModel.FindActiveByReaderIdAsync(
         readerId: loan2.ReaderId,
         ct: ct
      );

      // Assert
      result.IsSuccess.Should().BeTrue();

      var actualLoanDtos = result.Value;

      actualLoanDtos.Should().NotBeNull();
      actualLoanDtos.Count.Should().Be(expLoanIds.Count);

      var actualLoanIds = actualLoanDtos
         .Select(l => l.Id)
         .ToList();

      actualLoanIds.Should().BeEquivalentTo(
         expLoanIds,
         options => options.WithStrictOrdering()
      );

      actualLoanDtos.Should().OnlyContain(l =>
         l.ReaderId == loan2.ReaderId &&
         l.Status == LoanStatus.Active);
   }

   [Fact]
   public async Task FindActiveByReaderIdAsync_reader_without_active_loans_returns_empty_list() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var loanRepository = scope.ServiceProvider.GetRequiredService<ILoanRepository>();
      var readModel = scope.ServiceProvider.GetRequiredService<ILoanReadModel>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var loans = seed.Loans;
      var readerWithoutLoansId = Guid.Parse("00000006-0000-0000-0000-000000000000");

      AddLoans(
         loanRepository: loanRepository,
         loans: loans
      );

      await unitOfWork.SaveAllChangesAsync(
         "Loans inserted",
         ct
      );

      unitOfWork.ClearChangeTracker();

      // Act
      var result = await readModel.FindActiveByReaderIdAsync(
         readerId: readerWithoutLoansId,
         ct: ct
      );

      // Assert
      result.IsSuccess.Should().BeTrue();
      result.Value.Should().BeEmpty();
   }

   [Fact]
   public async Task FindActiveByReaderIdAsync_empty_reader_id_returns_failure() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var readModel = scope.ServiceProvider.GetRequiredService<ILoanReadModel>();

      // Act
      var result = await readModel.FindActiveByReaderIdAsync(
         readerId: Guid.Empty,
         ct: ct
      );

      // Assert
      result.IsFailure.Should().BeTrue();
      result.Error.Should().Be(LoanErrors.ReaderIdRequired);
   }

   [Fact]
   public async Task FindAllOverdueAsync_ok() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var loanRepository = scope.ServiceProvider.GetRequiredService<ILoanRepository>();
      var readModel = scope.ServiceProvider.GetRequiredService<ILoanReadModel>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var loans = seed.Loans;

      AddLoans(
         loanRepository: loanRepository,
         loans: loans
      );

      await unitOfWork.SaveAllChangesAsync(
         "Loans inserted",
         ct
      );

      unitOfWork.ClearChangeTracker();

      var utcNow = loans
         .Min(l => l.DueDate)
         .AddDays(1);

      var expLoanIds = loans
         .Where(l =>
            l.Status == LoanStatus.Active &&
            l.DueDate < utcNow)
         .OrderBy(l => l.DueDate)
         .Select(l => l.Id)
         .ToList();

      // Act
      var result = await readModel.FindAllOverdueAsync(
         utcNow: utcNow,
         ct: ct
      );

      // Assert
      result.IsSuccess.Should().BeTrue();

      var actualLoanDtos = result.Value;

      actualLoanDtos.Should().NotBeNull();

      var actualLoanIds = actualLoanDtos
         .Select(l => l.Id)
         .ToList();

      actualLoanIds.Should().BeEquivalentTo(
         expLoanIds,
         options => options.WithStrictOrdering()
      );

      actualLoanDtos.Should().OnlyContain(l =>
         l.Status == LoanStatus.Active &&
         l.DueDate < utcNow);
   }

   [Fact]
   public async Task FindAllOverdueAsync_does_not_return_loan_due_exactly_at_utc_now() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var loanRepository = scope.ServiceProvider.GetRequiredService<ILoanRepository>();
      var readModel = scope.ServiceProvider.GetRequiredService<ILoanReadModel>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var loans = seed.Loans;
      var loanDueExactlyAtUtcNow = loans[1];

      AddLoans(
         loanRepository: loanRepository,
         loans: loans
      );

      await unitOfWork.SaveAllChangesAsync(
         "Loans inserted",
         ct
      );

      unitOfWork.ClearChangeTracker();

      // Act
      var result = await readModel.FindAllOverdueAsync(
         utcNow: loanDueExactlyAtUtcNow.DueDate,
         ct: ct
      );

      // Assert
      result.IsSuccess.Should().BeTrue();

      result.Value
         .Select(l => l.Id)
         .Should()
         .NotContain(loanDueExactlyAtUtcNow.Id);
   }

   [Fact]
   public async Task FindAllOverdueAsync_default_utc_now_returns_failure() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var readModel = scope.ServiceProvider.GetRequiredService<ILoanReadModel>();

      // Act
      var result = await readModel.FindAllOverdueAsync(
         utcNow: default,
         ct: ct
      );

      // Assert
      result.IsFailure.Should().BeTrue();
      result.Error.Should().Be(LoanErrors.InvalidUtcNow);
   }

   [Fact]
   public async Task FindAllOverdueAsync_non_utc_now_returns_failure() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var readModel = scope.ServiceProvider.GetRequiredService<ILoanReadModel>();

      // Arrange
      var nonUtcNow = DateTime.SpecifyKind(
         DateTime.UtcNow,
         DateTimeKind.Unspecified
      );

      // Act
      var result = await readModel.FindAllOverdueAsync(
         utcNow: nonUtcNow,
         ct: ct
      );

      // Assert
      result.IsFailure.Should().BeTrue();
      result.Error.Should().Be(LoanErrors.InvalidUtcNow);
   }

   private static void AddLoans(
      ILoanRepository loanRepository,
      IReadOnlyList<Loan> loans
   ) {
      foreach (var loan in loans) {
         loanRepository.Add(
            loan: loan
         );
      }
   }
}