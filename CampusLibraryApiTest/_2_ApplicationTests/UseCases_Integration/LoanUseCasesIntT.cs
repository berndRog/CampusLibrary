using AwesomeAssertions;
using CampusLibraryApi._2_BuildingBlocks._1_Ports;
using CampusLibraryApi._3_Core.Catalog._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Catalog._3_Domain.Errors;
using CampusLibraryApi._3_Core.Loans._1_Ports.Inbound;
using CampusLibraryApi._3_Core.Loans._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Loans._2_Application.Dtos;
using CampusLibraryApi._3_Core.Loans._3_Domain.Enums;
using CampusLibraryApi._3_Core.Loans._3_Domain.Errors;
using CampusLibraryApi._3_Core.Loans._3_Domain.Policies;
using CampusLibraryApi._3_Core.Readers._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Readers._3_Domain.Errors;
using CampusLibraryApiTest.TestInfrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace CampusLibraryApiTest._2_ApplicationTests.UseCases_Integration;

public sealed class LoanUseCasesIntT : TestBaseIntegration {

   public LoanUseCasesIntT() {
      DbName = nameof(LoanUseCasesIntT);
      DbMode = DbMode.InMemory;
      SensitiveDataLogging = true;
   }

   #region LoanUcBorrow
   [Fact]
   public async Task BorrowAsync_ok_persists_loan_to_database() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var useCases = scope.ServiceProvider.GetRequiredService<ILoanUseCases>();
      var readModel = scope.ServiceProvider.GetRequiredService<ILoanReadModel>();
      var readerRepository = scope.ServiceProvider.GetRequiredService<IReaderRepository>();
      var bookRepository = scope.ServiceProvider.GetRequiredService<IBookRepository>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var reader1 = seed.Reader1();
      var books = seed.Books;
      var book1 = books[0];

      var bookItem1 = book1.BookItems.Single(bi =>
         bi.Id == Guid.Parse(seed.BookItem1Id));

      readerRepository.Add(reader1);
      bookRepository.AddRange(books);
      await unitOfWork.SaveAllChangesAsync("Reader and books inserted", ct);
      unitOfWork.ClearChangeTracker();

      var dto = new LoanCreateDto(
         Id: seed.Loan1Id,
         ReaderId: reader1.Id,
         BookItemId: bookItem1.Id
      );

      // Act
      var resultBorrow = await useCases.BorrowAsync(
         dto: dto,
         ct: ct
      );

      // Assert
      resultBorrow.IsSuccess.Should().BeTrue();

      var createdLoanDto = resultBorrow.Value;

      createdLoanDto.Id.Should().Be(Guid.Parse(seed.Loan1Id));
      createdLoanDto.ReaderId.Should().Be(reader1.Id);
      createdLoanDto.BookItemId.Should().Be(bookItem1.Id);
      createdLoanDto.ReturnedAt.Should().BeNull();
      createdLoanDto.Status.Should().Be((int)LoanStatus.Borrowed);
      createdLoanDto.RenewalCount.Should().Be(0);

      createdLoanDto.DueDate.Should().Be(
         createdLoanDto.LoanDate.AddDays(LoanRules.StandardLoanDays)
      );

      unitOfWork.ClearChangeTracker();

      var resultFind = await readModel.FindByIdAsync(
         id: createdLoanDto.Id,
         ct: ct
      );

      resultFind.IsSuccess.Should().BeTrue();

      var actualLoanDto = resultFind.Value;

      actualLoanDto.Id.Should().Be(createdLoanDto.Id);
      actualLoanDto.ReaderId.Should().Be(reader1.Id);
      actualLoanDto.BookItemId.Should().Be(bookItem1.Id);
      actualLoanDto.Status.Should().Be((int)LoanStatus.Borrowed);
      actualLoanDto.ReturnedAt.Should().BeNull();
   }

   [Fact]
   public async Task BorrowAsync_unknown_reader_fails_and_does_not_insert_loan() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var useCases = scope.ServiceProvider.GetRequiredService<ILoanUseCases>();
      var readModel = scope.ServiceProvider.GetRequiredService<ILoanReadModel>();
      var bookRepository = scope.ServiceProvider.GetRequiredService<IBookRepository>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var books = seed.Books;
      var book1 = books[0];
      var bookItem1 = book1.BookItems.Single(bi =>
         bi.Id == Guid.Parse(seed.BookItem1Id));
      var loan1Id = Guid.Parse(seed.Loan1Id);

      bookRepository.AddRange( books);
      await unitOfWork.SaveAllChangesAsync("Books inserted", ct);
      unitOfWork.ClearChangeTracker();

      var unknownReaderId = Guid.Parse("99999999-0000-0000-0000-000000000000");

      var dto = new LoanCreateDto(
         Id: seed.Loan1Id,
         ReaderId: unknownReaderId,
         BookItemId: bookItem1.Id
      );

      // Act
      var resultBorrow = await useCases.BorrowAsync(dto, ct);

      // Assert
      resultBorrow.IsFailure.Should().BeTrue();
      resultBorrow.Error.Should().Be(CommonErrors.ReaderNotFound);

      var resultFind = await readModel.FindByIdAsync(loan1Id, ct);

      resultFind.IsFailure.Should().BeTrue();
   }

   [Fact]
   public async Task BorrowAsync_inactive_reader_fails_and_does_not_insert_loan() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var useCases = scope.ServiceProvider.GetRequiredService<ILoanUseCases>();
      var readModel = scope.ServiceProvider.GetRequiredService<ILoanReadModel>();
      var readerRepository = scope.ServiceProvider.GetRequiredService<IReaderRepository>();
      var bookRepository = scope.ServiceProvider.GetRequiredService<IBookRepository>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var reader1 = seed.Reader1();
      var books = seed.Books;
      var book1 = books[0];

      var bookItem1 = book1.BookItems.Single(bi =>
         bi.Id == Guid.Parse(seed.BookItem1Id));

      readerRepository.Add(reader1);
      bookRepository.AddRange(books);
      await unitOfWork.SaveAllChangesAsync("Reader and books inserted", ct);

      var resultDeactivated = reader1.Deactivate(
         updatedAt: reader1.CreatedAt.AddDays(1)
      );
      resultDeactivated.IsSuccess.Should().BeTrue();

      await unitOfWork.SaveAllChangesAsync("Reader deactivated", ct);
      unitOfWork.ClearChangeTracker();

      var dto = new LoanCreateDto(
         Id: seed.Loan1Id,
         ReaderId: reader1.Id,
         BookItemId: bookItem1.Id
      );

      // Act
      var resultBorrow = await useCases.BorrowAsync(
         dto: dto,
         ct: ct
      );

      // Assert
      resultBorrow.IsFailure.Should().BeTrue();
      resultBorrow.Error.Should().Be(CommonErrors.ReaderIsDeactivated);

      var resultFind = await readModel.FindByIdAsync(
         id: Guid.Parse(seed.Loan1Id),
         ct: ct
      );

      resultFind.IsFailure.Should().BeTrue();
   }

   [Fact]
   public async Task BorrowAsync_unknown_book_item_fails_and_does_not_insert_loan() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var useCases = scope.ServiceProvider.GetRequiredService<ILoanUseCases>();
      var readModel = scope.ServiceProvider.GetRequiredService<ILoanReadModel>();
      var readerRepository = scope.ServiceProvider.GetRequiredService<IReaderRepository>();
      var bookRepository = scope.ServiceProvider.GetRequiredService<IBookRepository>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var reader1 = seed.Reader1();
      var books = seed.Books;
      var loan1Id = Guid.Parse(seed.Loan1Id);

      readerRepository.Add(reader1);
      bookRepository.AddRange(books);
      await unitOfWork.SaveAllChangesAsync("Reader and books inserted", ct);
      unitOfWork.ClearChangeTracker();

      var unknownBookItemId = Guid.Parse("be999999-0000-0000-0000-000000000000");

      var dto = new LoanCreateDto(
         Id: seed.Loan1Id,
         ReaderId: reader1.Id,
         BookItemId: unknownBookItemId
      );

      // Act
      var resultBorrow = await useCases.BorrowAsync(dto, ct);

      // Assert
      resultBorrow.IsFailure.Should().BeTrue();
      resultBorrow.Error.Should().Be(CommonErrors.BookItemNotFound);

      var resultFind = await readModel.FindByIdAsync(loan1Id, ct);

      resultFind.IsFailure.Should().BeTrue();
   }

   [Fact]
   public async Task BorrowAsync_deactivated_book_fails_and_does_not_insert_loan() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var useCases = scope.ServiceProvider.GetRequiredService<ILoanUseCases>();
      var readModel = scope.ServiceProvider.GetRequiredService<ILoanReadModel>();
      var readerRepository = scope.ServiceProvider.GetRequiredService<IReaderRepository>();
      var bookRepository = scope.ServiceProvider.GetRequiredService<IBookRepository>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var reader1 = seed.Reader1();
      var books = seed.Books;
      var book1 = books[0];

      var bookItem1 = book1.BookItems.Single(bi =>
         bi.Id == Guid.Parse(seed.BookItem1Id));

      readerRepository.Add(reader1);
      bookRepository.AddRange(books);
      await unitOfWork.SaveAllChangesAsync("Reader and books inserted", ct);

      var resultDeactivated = book1.Deactivate(
         updatedAt: book1.CreatedAt.AddDays(1)
      );

      resultDeactivated.IsSuccess.Should().BeTrue();

      await unitOfWork.SaveAllChangesAsync("Book deactivated", ct);
      unitOfWork.ClearChangeTracker();

      var dto = new LoanCreateDto(
         Id: seed.Loan1Id,
         ReaderId: reader1.Id,
         BookItemId: bookItem1.Id
      );

      // Act
      var resultBorrow = await useCases.BorrowAsync(
         dto: dto,
         ct: ct
      );

      // Assert
      resultBorrow.IsFailure.Should().BeTrue();
      resultBorrow.Error.Should().Be(LoanErrors.BookItemNotAvailable);

      var resultFind = await readModel.FindByIdAsync(
         id: Guid.Parse(seed.Loan1Id),
         ct: ct
      );

      resultFind.IsFailure.Should().BeTrue();
   }

   [Fact]
   public async Task BorrowAsync_book_item_already_borrowed_fails() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var useCases = scope.ServiceProvider.GetRequiredService<ILoanUseCases>();
      var readerRepository = scope.ServiceProvider.GetRequiredService<IReaderRepository>();
      var bookRepository = scope.ServiceProvider.GetRequiredService<IBookRepository>();
      var loanRepository = scope.ServiceProvider.GetRequiredService<ILoanRepository>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var readers = seed.Readers;
      var books = seed.Books;
      var existingLoan = seed.Loan1();

      readerRepository.AddRange(
         readers: readers
      );

      bookRepository.AddRange(
         books: books
      );

      loanRepository.Add(
         loan: existingLoan
      );

      await unitOfWork.SaveAllChangesAsync(
         "Readers, books and existing loan inserted",
         ct
      );

      unitOfWork.ClearChangeTracker();

      var dto = new LoanCreateDto(
         Id: "a1000009-0000-0000-0000-000000000000",
         ReaderId: existingLoan.ReaderId,
         BookItemId: existingLoan.BookItemId
      );

      // Act
      var resultBorrow = await useCases.BorrowAsync(
         dto: dto,
         ct: ct
      );

      // Assert
      resultBorrow.IsFailure.Should().BeTrue();
      resultBorrow.Error.Should().Be(LoanErrors.BookItemAlreadyBorrowed);
   }
   #endregion

   #region LoanUcReturnAtDesk
   [Fact]
   public async Task ReturnAtDeskAsync_ok_persists_returned_status_and_returned_at() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var useCases = scope.ServiceProvider.GetRequiredService<ILoanUseCases>();
      var loanRepository = scope.ServiceProvider.GetRequiredService<ILoanRepository>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var loan1 = seed.Loan1();

      loanRepository.Add(
         loan: loan1
      );

      await unitOfWork.SaveAllChangesAsync(
         "Loan inserted",
         ct
      );

      unitOfWork.ClearChangeTracker();

      // Act
      var resultReturn = await useCases.ReturnAtDeskAsync(
         loanId: loan1.Id,
         ct: ct
      );

      // Assert
      resultReturn.IsSuccess.Should().BeTrue();

      var returnedLoanDto = resultReturn.Value;

      returnedLoanDto.Id.Should().Be(loan1.Id);
      returnedLoanDto.Status.Should().Be((int)LoanStatus.Returned);
      returnedLoanDto.ReturnedAt.Should().NotBeNull();

      unitOfWork.ClearChangeTracker();

      var actualLoan = await loanRepository.FindByIdAsync(
         id: loan1.Id,
         ct: ct
      );

      actualLoan.Should().NotBeNull();
      actualLoan!.Status.Should().Be(LoanStatus.Returned);
      actualLoan.ReturnedAt.Should().NotBeNull();
   }

   [Fact]
   public async Task ReturnAtDeskAsync_unknown_loan_fails() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var useCases = scope.ServiceProvider.GetRequiredService<ILoanUseCases>();

      // Arrange
      var unknownLoanId = Guid.Parse("a1999999-0000-0000-0000-000000000000");

      // Act
      var resultReturn = await useCases.ReturnAtDeskAsync(
         loanId: unknownLoanId,
         ct: ct
      );

      // Assert
      resultReturn.IsFailure.Should().BeTrue();
      resultReturn.Error.Should().Be(LoanErrors.LoanNotFound);
   }

   [Fact]
   public async Task ReturnAtDeskAsync_empty_loan_id_fails() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var useCases = scope.ServiceProvider.GetRequiredService<ILoanUseCases>();

      // Act
      var resultReturn = await useCases.ReturnAtDeskAsync(
         loanId: Guid.Empty,
         ct: ct
      );

      // Assert
      resultReturn.IsFailure.Should().BeTrue();
      resultReturn.Error.Should().Be(LoanErrors.LoanIdRequired);
   }

   [Fact]
   public async Task ReturnAtDeskAsync_already_returned_loan_fails() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var useCases = scope.ServiceProvider.GetRequiredService<ILoanUseCases>();
      var loanRepository = scope.ServiceProvider.GetRequiredService<ILoanRepository>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var loan1 = seed.Loan1();

      loanRepository.Add(
         loan: loan1
      );

      await unitOfWork.SaveAllChangesAsync(
         "Loan inserted",
         ct
      );

      var resultReturned = loan1.ReturnAtDesk(
         returnedAt: loan1.LoanDate.AddDays(1)
      );

      resultReturned.IsSuccess.Should().BeTrue();

      await unitOfWork.SaveAllChangesAsync(
         "Loan returned",
         ct
      );

      unitOfWork.ClearChangeTracker();

      // Act
      var resultReturn = await useCases.ReturnAtDeskAsync(
         loanId: loan1.Id,
         ct: ct
      );

      // Assert
      resultReturn.IsFailure.Should().BeTrue();
      resultReturn.Error.Should().Be(LoanErrors.LoanAlreadyReturned);
   }
   #endregion

   #region LoanUcRenew
   [Fact]
   public async Task RenewAsync_ok_persists_new_due_date_and_renewal_count() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var useCases = scope.ServiceProvider.GetRequiredService<ILoanUseCases>();
      var loanRepository = scope.ServiceProvider.GetRequiredService<ILoanRepository>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var loan1 = seed.Loan1();
      var oldDueDate = loan1.DueDate;

      loanRepository.Add(
         loan: loan1
      );

      await unitOfWork.SaveAllChangesAsync(
         "Loan inserted",
         ct
      );

      unitOfWork.ClearChangeTracker();

      // Act
      var resultRenew = await useCases.RenewAsync(
         loanId: loan1.Id,
         ct: ct
      );

      // Assert
      resultRenew.IsSuccess.Should().BeTrue();

      var renewedLoanDto = resultRenew.Value;

      renewedLoanDto.Id.Should().Be(loan1.Id);
      renewedLoanDto.Status.Should().Be((int)LoanStatus.Borrowed);
      renewedLoanDto.RenewalCount.Should().Be(1);
      renewedLoanDto.DueDate.Should().Be(
         oldDueDate.AddDays(LoanRules.StandardRenewalDays)
      );

      unitOfWork.ClearChangeTracker();

      var actualLoan = await loanRepository.FindByIdAsync(
         id: loan1.Id,
         ct: ct
      );

      actualLoan.Should().NotBeNull();
      actualLoan!.RenewalCount.Should().Be(1);
      actualLoan.DueDate.Should().Be(
         oldDueDate.AddDays(LoanRules.StandardRenewalDays)
      );
   }

   [Fact]
   public async Task RenewAsync_unknown_loan_fails() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var useCases = scope.ServiceProvider.GetRequiredService<ILoanUseCases>();

      // Arrange
      var unknownLoanId = Guid.Parse("a1999999-0000-0000-0000-000000000000");

      // Act
      var resultRenew = await useCases.RenewAsync(
         loanId: unknownLoanId,
         ct: ct
      );

      // Assert
      resultRenew.IsFailure.Should().BeTrue();
      resultRenew.Error.Should().Be(LoanErrors.LoanNotFound);
   }

   [Fact]
   public async Task RenewAsync_empty_loan_id_fails() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var useCases = scope.ServiceProvider.GetRequiredService<ILoanUseCases>();

      // Act
      var resultRenew = await useCases.RenewAsync(
         loanId: Guid.Empty,
         ct: ct
      );

      // Assert
      resultRenew.IsFailure.Should().BeTrue();
      resultRenew.Error.Should().Be(LoanErrors.LoanIdRequired);
   }

   [Fact]
   public async Task RenewAsync_returned_loan_fails() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var useCases = scope.ServiceProvider.GetRequiredService<ILoanUseCases>();
      var loanRepository = scope.ServiceProvider.GetRequiredService<ILoanRepository>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var loan1 = seed.Loan1();

      loanRepository.Add(
         loan: loan1
      );

      await unitOfWork.SaveAllChangesAsync(
         "Loan inserted",
         ct
      );

      var resultReturned = loan1.ReturnAtDesk(
         returnedAt: loan1.LoanDate.AddDays(1)
      );

      resultReturned.IsSuccess.Should().BeTrue();

      await unitOfWork.SaveAllChangesAsync(
         "Loan returned",
         ct
      );

      unitOfWork.ClearChangeTracker();

      // Act
      var resultRenew = await useCases.RenewAsync(
         loanId: loan1.Id,
         ct: ct
      );

      // Assert
      resultRenew.IsFailure.Should().BeTrue();
      resultRenew.Error.Should().Be(LoanErrors.LoanAlreadyReturned);
   }

   [Fact]
   public async Task RenewAsync_overdue_loan_fails() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var useCases = scope.ServiceProvider.GetRequiredService<ILoanUseCases>();
      var loanRepository = scope.ServiceProvider.GetRequiredService<ILoanRepository>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var overdueLoan = seed.Loan3();

      loanRepository.Add(
         loan: overdueLoan
      );

      await unitOfWork.SaveAllChangesAsync(
         "Overdue loan inserted",
         ct
      );

      unitOfWork.ClearChangeTracker();

      // Act
      var resultRenew = await useCases.RenewAsync(
         loanId: overdueLoan.Id,
         ct: ct
      );

      // Assert
      resultRenew.IsFailure.Should().BeTrue();
      resultRenew.Error.Should().Be(LoanErrors.LoanAlreadyOverdue);
   }
   #endregion
}