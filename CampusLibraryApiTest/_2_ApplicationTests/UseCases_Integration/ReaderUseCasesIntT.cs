using AwesomeAssertions;
using CampusLibraryApi._2_BuildingBlocks._1_Ports;
using CampusLibraryApi._3_Core.Catalog._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Loans._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Readers._1_Ports.Inbound;
using CampusLibraryApi._3_Core.Readers._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Readers._3_Domain.Errors;
using CampusLibraryApiTest.TestInfrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace CampusLibraryApiTest._2_ApplicationTests.UseCases_Integration;

public sealed class ReaderUseCasesIntT : TestBaseIntegration {

   public ReaderUseCasesIntT() {
      DbName = nameof(ReaderUseCasesIntT);
      DbMode = DbMode.InMemory;
      SensitiveDataLogging = true;
   }

   #region ReaderUcDeactivate

   [Fact]
   public async Task DeactivateAsync_ok_hides_reader_from_normal_queries() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;

      var useCases = scope.ServiceProvider.GetRequiredService<IReaderUseCases>();
      var repository = scope.ServiceProvider.GetRequiredService<IReaderRepository>();
      var readModel = scope.ServiceProvider.GetRequiredService<IReaderReadModel>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var reader = seed.Reader1();

      repository.Add(reader);
      await unitOfWork.SaveAllChangesAsync("Reader1 inserted", ct);
      unitOfWork.ClearChangeTracker();

      // Act
      var resultDeactivate = await useCases.DeactivateAsync(
         id: reader.Id,
         ct: ct
      );

      resultDeactivate.IsSuccess.Should().BeTrue();

      unitOfWork.ClearChangeTracker();

      // Assert: normal read model queries no longer return inactive readers.
      var activeFindResult = await readModel.FindByIdAsync(
         id: reader.Id,
         ct: ct
      );

      activeFindResult.IsFailure.Should().BeTrue();
      activeFindResult.Error.Should().Be(ReaderErrors.ReaderNotFound);

      // Assert: administrative/internal queries can still find the reader.
      var inactiveFindResult = await readModel.FindByIdAsync(
         id: reader.Id,
         includeInactive: true,
         ct: ct
      );

      inactiveFindResult.IsSuccess.Should().BeTrue();
      inactiveFindResult.Value.Id.Should().Be(reader.Id);
      inactiveFindResult.Value.IsActive.Should().BeFalse();
   }

   [Fact]
   public async Task DeactivateAsync_with_current_loan_fails_then_succeeds_after_return() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;

      var useCases = scope.ServiceProvider.GetRequiredService<IReaderUseCases>();
      var readerRepository = scope.ServiceProvider.GetRequiredService<IReaderRepository>();
      var bookRepository = scope.ServiceProvider.GetRequiredService<IBookRepository>();
      var loanRepository = scope.ServiceProvider.GetRequiredService<ILoanRepository>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var reader = seed.Reader1();
      var book = seed.Books.Single(candidate =>
         candidate.BookItems.Any(item => item.Id == Guid.Parse(seed.BookItem1Id))
      );
      var loan = seed.Loan1();

      readerRepository.Add(reader);
      bookRepository.Add(book);
      loanRepository.Add(loan);

      await unitOfWork.SaveAllChangesAsync(
         "Reader, book and loan inserted",
         ct
      );
      unitOfWork.ClearChangeTracker();

      // Act 1: current loan prevents deactivation.
      var resultWithLoan = await useCases.DeactivateAsync(
         id: reader.Id,
         ct: ct
      );

      // Assert 1
      resultWithLoan.IsFailure.Should().BeTrue();
      resultWithLoan.Error.Should().Be(
         ReaderErrors.ReaderCannotBeDeactivatedWithLoans
      );

      unitOfWork.ClearChangeTracker();

      // Arrange 2: return at the desk deletes the current Loan.
      var persistedLoan = await loanRepository.FindByIdAsync(
         id: loan.Id,
         ct: ct
      );

      persistedLoan.Should().NotBeNull();
      loanRepository.Remove(persistedLoan!);

      await unitOfWork.SaveAllChangesAsync(
         "Loan returned and deleted",
         ct
      );
      unitOfWork.ClearChangeTracker();

      // Act 2
      var resultAfterReturn = await useCases.DeactivateAsync(
         id: reader.Id,
         ct: ct
      );

      // Assert 2
      resultAfterReturn.IsSuccess.Should().BeTrue();
   }

   [Fact]
   public async Task DeactivateAsync_unknown_reader_fails() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;

      var useCases = scope.ServiceProvider.GetRequiredService<IReaderUseCases>();

      // Arrange
      var unknownId = Guid.Parse("99000000-0000-0000-0000-000000000000");

      // Act
      var deactivateResult = await useCases.DeactivateAsync(
         id: unknownId,
         ct: ct
      );

      // Assert
      deactivateResult.IsFailure.Should().BeTrue();
      deactivateResult.Error.Should().Be(ReaderErrors.ReaderNotFound);
   }

   #endregion
}
