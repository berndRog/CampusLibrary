using AwesomeAssertions;
using CampusLibraryApi._2_BuildingBlocks._1_Ports;
using CampusLibraryApi._3_Core.Catalog._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Loans._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Loans._3_Domain.Errors;
using CampusLibraryApi._3_Core.Readers._1_Ports.Outbound;
using CampusLibraryApiTest.TestInfrastructure;
using Microsoft.Extensions.DependencyInjection;
using CampusLibraryApi._2_BuildingBlocks._3_Domain.Errors;

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
      var readModel = scope.ServiceProvider.GetRequiredService<ILoanReadModel>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var readers = seed.Readers;
      var books = seed.Books;
      var loans = seed.Loans;

      var loan1 = loans[0];

      var reader = readers.Single(r =>
         r.Id == loan1.ReaderId
      );

      var book = books.Single(b =>
         b.BookItems.Any(bi => bi.Id == loan1.BookItemId)
      );

      var bookItem = book.BookItems.Single(bi =>
         bi.Id == loan1.BookItemId
      );

      await InsertLoanReadModelDataAsync(
         scope: scope,
         readers: readers,
         books: books,
         loans: loans,
         ct: ct
      );

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
      actualLoanDto.Firstname.Should().Be(reader.Firstname);
      actualLoanDto.Lastname.Should().Be(reader.Lastname);

      actualLoanDto.BookItemId.Should().Be(loan1.BookItemId);
      actualLoanDto.BookId.Should().Be(book.Id);

      actualLoanDto.Title.Should().Be(book.Title);
      actualLoanDto.Subtitle.Should().Be(book.Subtitle);
      actualLoanDto.AuthorsText.Should().Be(book.AuthorsText);
      actualLoanDto.Isbn.Should().Be(book.IsbnVo.Value);

      actualLoanDto.BookIsActive.Should().Be(book.IsActive);
      actualLoanDto.IsAvailableForLoan.Should().BeTrue();

      actualLoanDto.LoanDate.Should().Be(loan1.LoanDate);
      actualLoanDto.DueDate.Should().Be(loan1.DueDate);

      actualLoanDto.RenewalCount.Should().Be(loan1.RenewalCount);

      actualLoanDto.IsOverdue.Should().BeFalse();
      actualLoanDto.CanRenew.Should().BeTrue();
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
      result.Error.Should().Be(LoanErrors.InvalidLoanId);
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
   public async Task FindAllBorrowedAsync_ok() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var readModel = scope.ServiceProvider.GetRequiredService<ILoanReadModel>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var readers = seed.Readers;
      var books = seed.Books;
      var loans = seed.Loans;

      await InsertLoanReadModelDataAsync(
         scope: scope,
         readers: readers,
         books: books,
         loans: loans,
         ct: ct
      );

      var expLoanIds = loans
                  .OrderBy(l => l.DueDate)
         .ThenBy(l => l.LoanDate)
         .ThenBy(l => l.Id)
         .Select(l => l.Id)
         .ToList();

      // Act
      var result = await readModel.FindAllBorrowedAsync(ct);

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

   }

   [Fact]
   public async Task FindAllBorrowedAsync_returns_reader_and_book_item_data() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var readModel = scope.ServiceProvider.GetRequiredService<ILoanReadModel>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var readers = seed.Readers;
      var books = seed.Books;
      var loans = seed.Loans;

      var loan1 = loans[0];

      var reader = readers.Single(r =>
         r.Id == loan1.ReaderId
      );

      var book = books.Single(b =>
         b.BookItems.Any(bi => bi.Id == loan1.BookItemId)
      );

      var bookItem = book.BookItems.Single(bi =>
         bi.Id == loan1.BookItemId
      );

      await InsertLoanReadModelDataAsync(
         scope: scope,
         readers: readers,
         books: books,
         loans: loans,
         ct: ct
      );

      // Act
      var result = await readModel.FindAllBorrowedAsync(
         ct: ct
      );

      // Assert
      result.IsSuccess.Should().BeTrue();

      var actualLoanDto = result.Value.Single(l =>
         l.Id == loan1.Id
      );

      actualLoanDto.ReaderId.Should().Be(reader.Id);
      actualLoanDto.Firstname.Should().Be(reader.Firstname);
      actualLoanDto.Lastname.Should().Be(reader.Lastname);

      actualLoanDto.BookItemId.Should().Be(bookItem.Id);

      actualLoanDto.Title.Should().Be(book.Title);
      actualLoanDto.Subtitle.Should().Be(book.Subtitle);

      actualLoanDto.LoanDate.Should().Be(loan1.LoanDate);
      actualLoanDto.DueDate.Should().Be(loan1.DueDate);

      actualLoanDto.IsOverdue.Should().BeFalse();
   }

   [Fact]
   public async Task FindAllBorrowedAsync_deactivated_reader_keeps_existing_loan_visible() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var readModel = scope.ServiceProvider.GetRequiredService<ILoanReadModel>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var readers = seed.Readers.ToList();
      var loan = seed.Loan1();
      var reader = readers.Single(candidate =>
         candidate.Id == loan.ReaderId
      );

      var deactivateResult = reader.Deactivate(
         updatedAt: reader.CreatedAt.AddDays(1)
      );
      deactivateResult.IsSuccess.Should().BeTrue();

      await InsertLoanReadModelDataAsync(
         scope: scope,
         readers: readers,
         books: seed.Books,
         loans: [loan],
         ct: ct
      );

      // Act
      var result = await readModel.FindAllBorrowedAsync(
         ct: ct
      );

      // Assert
      result.IsSuccess.Should().BeTrue();
      result.Value.Should().ContainSingle(item =>
         item.Id == loan.Id && item.ReaderId == reader.Id
      );
   }

   [Fact]
   public async Task FindBorrowedByReaderIdAsync_returns_only_reader_loans() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var readModel = scope.ServiceProvider.GetRequiredService<ILoanReadModel>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      var loans = seed.Loans;
      var expectedLoan = loans[0];

      await InsertLoanReadModelDataAsync(
         scope: scope,
         readers: seed.Readers,
         books: seed.Books,
         loans: loans,
         ct: ct
      );

      var result = await readModel.FindBorrowedByReaderIdAsync(
         readerId: expectedLoan.ReaderId,
         ct: ct
      );

      result.IsSuccess.Should().BeTrue();
      result.Value.Should().ContainSingle();
      result.Value[0].Id.Should().Be(expectedLoan.Id);
      result.Value[0].ReaderId.Should().Be(expectedLoan.ReaderId);
   }

   [Fact]
   public async Task FindByIdForReaderAsync_other_reader_returns_not_found() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var readModel = scope.ServiceProvider.GetRequiredService<ILoanReadModel>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      var loan = seed.Loan1();
      var otherReader = seed.Reader2();

      await InsertLoanReadModelDataAsync(
         scope: scope,
         readers: seed.Readers,
         books: seed.Books,
         loans: [loan],
         ct: ct
      );

      var result = await readModel.FindByIdForReaderAsync(
         id: loan.Id,
         readerId: otherReader.Id,
         ct: ct
      );

      result.IsFailure.Should().BeTrue();
      result.Error.Should().Be(LoanErrors.LoanNotFound);
   }

   [Fact]
   public async Task FindAllBorrowedAsync_missing_reader_returns_failure() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var readerRepository = scope.ServiceProvider.GetRequiredService<IReaderRepository>();
      var bookRepository = scope.ServiceProvider.GetRequiredService<IBookRepository>();
      var loanRepository = scope.ServiceProvider.GetRequiredService<ILoanRepository>();
      var readModel = scope.ServiceProvider.GetRequiredService<ILoanReadModel>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var books = seed.Books;
      var loans = seed.Loans;
      bookRepository.AddRange(books);
      loanRepository.AddRange(loans);
      await unitOfWork.SaveAllChangesAsync("Books and loans inserted", ct);
      unitOfWork.ClearChangeTracker();

      // Act
      var result = await readModel.FindAllBorrowedAsync(ct);

      // Assert
      result.IsFailure.Should().BeTrue();
      result.Error.Should().Be(CommonErrors.ReaderNotFound);
   }

   [Fact]
   public async Task FindAllBorrowedAsync_missing_book_item_returns_failure() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var readerRepository = scope.ServiceProvider.GetRequiredService<IReaderRepository>();
      var loanRepository = scope.ServiceProvider.GetRequiredService<ILoanRepository>();
      var readModel = scope.ServiceProvider.GetRequiredService<ILoanReadModel>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var readers = seed.Readers;
      var loans = seed.Loans;

      readerRepository.AddRange(readers);
      loanRepository.AddRange(loans);

      await unitOfWork.SaveAllChangesAsync("Readers and loans inserted", ct);
      unitOfWork.ClearChangeTracker();

      // Act
      var result = await readModel.FindAllBorrowedAsync(ct);

      // Assert
      result.IsFailure.Should().BeTrue();
      result.Error.Should().Be(CommonErrors.BookItemNotFound);
   }

   private static async Task InsertLoanReadModelDataAsync(
      IServiceScope scope,
      IReadOnlyList<CampusLibraryApi._3_Core.Readers._3_Domain.Entities.Reader> readers,
      IReadOnlyList<CampusLibraryApi._3_Core.Catalog._3_Domain.Entities.Book> books,
      IReadOnlyList<CampusLibraryApi._3_Core.Loans._3_Domain.Entities.Loan> loans,
      CancellationToken ct
   ) {
      var readerRepository = scope.ServiceProvider
         .GetRequiredService<IReaderRepository>();

      var bookRepository = scope.ServiceProvider
         .GetRequiredService<IBookRepository>();

      var loanRepository = scope.ServiceProvider
         .GetRequiredService<ILoanRepository>();

      var unitOfWork = scope.ServiceProvider
         .GetRequiredService<IUnitOfWork>();

      readerRepository.AddRange(
         readers: readers
      );

      bookRepository.AddRange(
         books: books
      );

      loanRepository.AddRange(
         loans: loans
      );

      await unitOfWork.SaveAllChangesAsync(
         "Readers, books and loans inserted",
         ct
      );

      unitOfWork.ClearChangeTracker();
   }
}
