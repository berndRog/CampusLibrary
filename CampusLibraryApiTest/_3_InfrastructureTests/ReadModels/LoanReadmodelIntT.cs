using AwesomeAssertions;
using CampusLibraryApi._2_BuildingBlocks._1_Ports;
using CampusLibraryApi._3_Core.Catalog._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Loans._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Loans._3_Domain.Enums;
using CampusLibraryApi._3_Core.Loans._3_Domain.Errors;
using CampusLibraryApi._3_Core.Readers._1_Ports.Outbound;
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
      actualLoanDto.ReturnedAt.Should().Be(loan1.ReturnedAt);

      actualLoanDto.Status.Should().Be((int)loan1.Status);
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
         .Where(l =>
            l.Status == LoanStatus.Borrowed &&
            l.ReturnedAt is null)
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

      actualLoanDtos.Should().OnlyContain(l =>
         l.Status == (int)LoanStatus.Borrowed
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

      actualLoanDto.Status.Should().Be((int)LoanStatus.Borrowed);
      actualLoanDto.IsOverdue.Should().BeFalse();
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