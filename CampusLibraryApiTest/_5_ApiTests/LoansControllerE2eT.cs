using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;
using CampusLibraryApi._2_BuildingBlocks._1_Ports;
using CampusLibraryApi._3_Core.Catalog._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Loans._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Loans._2_Application.Dtos;
using CampusLibraryApi._3_Core.Loans._3_Domain.Enums;
using CampusLibraryApi._3_Core.Loans._3_Domain.Policies;
using CampusLibraryApi._3_Core.Readers._1_Ports.Outbound;
using CampusLibraryApiTest.TestController;
using CampusLibraryApiTest.TestInfrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace CampusLibraryApiTest._5_ApiTests;

public sealed class LoansControllerE2eT : TestBaseEndToEnd {

   protected override string DatabaseName => nameof(LoansControllerE2eT);
   protected override DbMode DbMode => DbMode.InMemory;

   private readonly string _url = "/camplib/v1";
   private readonly CancellationToken _ct = TestContext.Current.CancellationToken;

   [Fact]
   public async Task GetLoanByIdAsync_ok() {
      // Arrange
      Guid loanId = default;
      Guid readerId = default;
      Guid bookItemId = default;

      await Factory.WithScopeAsync(async sp => {
         var readerRepository = sp.GetRequiredService<IReaderRepository>();
         var bookRepository = sp.GetRequiredService<IBookRepository>();
         var loanRepository = sp.GetRequiredService<ILoanRepository>();
         var unitOfWork = sp.GetRequiredService<IUnitOfWork>();
         var seed = sp.GetRequiredService<TestSeed>();

         var readers = seed.Readers;
         var books = seed.Books;
         var loans = seed.Loans;
         var loan1 = loans[0];

         loanId = loan1.Id;
         readerId = loan1.ReaderId;
         bookItemId = loan1.BookItemId;

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
            _ct
         );

         unitOfWork.ClearChangeTracker();
      });

      // Act
      var response = await Client.GetAsync(
         $"{_url}/loans/{loanId}",
         _ct
      );

      // Assert
      response.StatusCode.Should().Be(HttpStatusCode.OK);

      var actualLoanDto = await response.Content
         .ReadFromJsonAsync<LoanDetailDto>(
            _ct
         );

      actualLoanDto.Should().NotBeNull();

      actualLoanDto!.Id.Should().Be(loanId);
      actualLoanDto.ReaderId.Should().Be(readerId);
      actualLoanDto.BookItemId.Should().Be(bookItemId);
      actualLoanDto.Status.Should().Be((int)LoanStatus.Borrowed);

      actualLoanDto.Firstname.Should().NotBeNullOrWhiteSpace();
      actualLoanDto.Lastname.Should().NotBeNullOrWhiteSpace();
      actualLoanDto.Title.Should().NotBeNullOrWhiteSpace();
      actualLoanDto.InventoryNumber.Should().NotBeNullOrWhiteSpace();

      actualLoanDto.ReturnedAt.Should().BeNull();
      actualLoanDto.IsOverdue.Should().BeFalse();
      actualLoanDto.CanRenew.Should().BeTrue();
   }

   [Fact]
   public async Task GetLoanByIdAsync_unknown_id_returns_not_found() {
      // Arrange
      var unknownLoanId = Guid.Parse("a1999999-0000-0000-0000-000000000000");

      // Act
      var response = await Client.GetAsync(
         $"{_url}/loans/{unknownLoanId}",
         _ct
      );

      // Assert
      response.StatusCode.Should().Be(HttpStatusCode.NotFound);
   }

   [Fact]
   public async Task GetBorrowedLoansAsync_ok() {
      // Arrange
      List<Guid> expectedLoanIds = [];

      await Factory.WithScopeAsync(async sp => {
         var readerRepository = sp.GetRequiredService<IReaderRepository>();
         var bookRepository = sp.GetRequiredService<IBookRepository>();
         var loanRepository = sp.GetRequiredService<ILoanRepository>();
         var unitOfWork = sp.GetRequiredService<IUnitOfWork>();
         var seed = sp.GetRequiredService<TestSeed>();

         var readers = seed.Readers;
         var books = seed.Books;
         var loans = seed.Loans;

         expectedLoanIds = loans
            .Where(l =>
               l.Status == LoanStatus.Borrowed &&
               l.ReturnedAt is null)
            .OrderBy(l => l.DueDate)
            .ThenBy(l => l.LoanDate)
            .ThenBy(l => l.Id)
            .Select(l => l.Id)
            .ToList();

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
            _ct
         );

         unitOfWork.ClearChangeTracker();
      });

      // Act
      var response = await Client.GetAsync(
         $"{_url}/loans",
         _ct
      );

      // Assert
      response.StatusCode.Should().Be(HttpStatusCode.OK);

      var actualLoanDtos = await response.Content
         .ReadFromJsonAsync<List<LoanListItemDto>>(
            _ct
         );

      actualLoanDtos.Should().NotBeNull();

      actualLoanDtos!
         .Select(l => l.Id)
         .Should()
         .BeEquivalentTo(
            expectedLoanIds,
            options => options.WithStrictOrdering()
         );

      actualLoanDtos.Should().OnlyContain(l =>
         l.Status == (int)LoanStatus.Borrowed
      );

      actualLoanDtos.Should().OnlyContain(l =>
         !string.IsNullOrWhiteSpace(l.Firstname) &&
         !string.IsNullOrWhiteSpace(l.Lastname) &&
         !string.IsNullOrWhiteSpace(l.Title) &&
         !string.IsNullOrWhiteSpace(l.InventoryNumber)
      );
   }

   [Fact]
   public async Task BorrowBookItemAsync_ok() {
      // Arrange
      LoanCreateDto dto = default!;
      Guid expectedReaderId = default;
      Guid expectedBookItemId = default;

      await Factory.WithScopeAsync(async sp => {
         var readerRepository = sp.GetRequiredService<IReaderRepository>();
         var bookRepository = sp.GetRequiredService<IBookRepository>();
         var unitOfWork = sp.GetRequiredService<IUnitOfWork>();
         var seed = sp.GetRequiredService<TestSeed>();

         var reader = seed.Reader1();
         var books = seed.Books;
         var book1 = books[0];

         var bookItem1 = book1.BookItems.Single(bi =>
            bi.Id == Guid.Parse(seed.BookItem1Id)
         );

         expectedReaderId = reader.Id;
         expectedBookItemId = bookItem1.Id;

         readerRepository.Add(
            reader: reader
         );

         bookRepository.AddRange(
            books: books
         );

         await unitOfWork.SaveAllChangesAsync(
            "Reader and books inserted",
            _ct
         );

         unitOfWork.ClearChangeTracker();

         dto = new LoanCreateDto(
            Id: seed.Loan1Id,
            ReaderId: reader.Id,
            BookItemId: bookItem1.Id
         );
      });

      // Act
      var response = await Client.PostAsJsonAsync(
         $"{_url}/loans",
         dto,
         _ct
      );

      // Assert
      response.StatusCode.Should().Be(HttpStatusCode.Created);
      response.Headers.Location.Should().NotBeNull();

      var actualLoanDto = await response.Content
         .ReadFromJsonAsync<LoanDto>(
            _ct
         );

      actualLoanDto.Should().NotBeNull();

      actualLoanDto!.Id.Should().Be(Guid.Parse(dto.Id!));
      actualLoanDto.ReaderId.Should().Be(expectedReaderId);
      actualLoanDto.BookItemId.Should().Be(expectedBookItemId);
      actualLoanDto.Status.Should().Be((int)LoanStatus.Borrowed);
      actualLoanDto.ReturnedAt.Should().BeNull();
      actualLoanDto.RenewalCount.Should().Be(0);

      actualLoanDto.DueDate.Should().Be(
         actualLoanDto.LoanDate.AddDays(LoanRules.StandardLoanDays)
      );
   }

   [Fact]
   public async Task BorrowBookItemAsync_book_item_already_borrowed_returns_conflict() {
      // Arrange
      LoanCreateDto dto = default!;

      await Factory.WithScopeAsync(async sp => {
         var readerRepository = sp.GetRequiredService<IReaderRepository>();
         var bookRepository = sp.GetRequiredService<IBookRepository>();
         var loanRepository = sp.GetRequiredService<ILoanRepository>();
         var unitOfWork = sp.GetRequiredService<IUnitOfWork>();
         var seed = sp.GetRequiredService<TestSeed>();

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
            _ct
         );

         unitOfWork.ClearChangeTracker();

         dto = new LoanCreateDto(
            Id: "a1000009-0000-0000-0000-000000000000",
            ReaderId: existingLoan.ReaderId,
            BookItemId: existingLoan.BookItemId
         );
      });

      // Act
      var response = await Client.PostAsJsonAsync(
         $"{_url}/loans",
         dto,
         _ct
      );

      // Assert
      response.StatusCode.Should().Be(HttpStatusCode.Conflict);
   }

   [Fact]
   public async Task BorrowBookItemAsync_unknown_reader_returns_not_found() {
      // Arrange
      LoanCreateDto dto = default!;

      await Factory.WithScopeAsync(async sp => {
         var bookRepository = sp.GetRequiredService<IBookRepository>();
         var unitOfWork = sp.GetRequiredService<IUnitOfWork>();
         var seed = sp.GetRequiredService<TestSeed>();

         var books = seed.Books;
         var book1 = books[0];

         var bookItem1 = book1.BookItems.Single(bi =>
            bi.Id == Guid.Parse(seed.BookItem1Id)
         );

         bookRepository.AddRange(
            books: books
         );

         await unitOfWork.SaveAllChangesAsync(
            "Books inserted",
            _ct
         );

         unitOfWork.ClearChangeTracker();

         dto = new LoanCreateDto(
            Id: seed.Loan1Id,
            ReaderId: Guid.Parse("99999999-0000-0000-0000-000000000000"),
            BookItemId: bookItem1.Id
         );
      });

      // Act
      var response = await Client.PostAsJsonAsync(
         $"{_url}/loans",
         dto,
         _ct
      );

      // Assert
      response.StatusCode.Should().Be(HttpStatusCode.NotFound);
   }

   [Fact]
   public async Task BorrowBookItemAsync_unknown_book_item_returns_not_found() {
      // Arrange
      LoanCreateDto dto = default!;

      await Factory.WithScopeAsync(async sp => {
         var readerRepository = sp.GetRequiredService<IReaderRepository>();
         var unitOfWork = sp.GetRequiredService<IUnitOfWork>();
         var seed = sp.GetRequiredService<TestSeed>();

         var reader = seed.Reader1();

         readerRepository.Add(
            reader: reader
         );

         await unitOfWork.SaveAllChangesAsync(
            "Reader inserted",
            _ct
         );

         unitOfWork.ClearChangeTracker();

         dto = new LoanCreateDto(
            Id: seed.Loan1Id,
            ReaderId: reader.Id,
            BookItemId: Guid.Parse("be999999-0000-0000-0000-000000000000")
         );
      });

      // Act
      var response = await Client.PostAsJsonAsync(
         $"{_url}/loans",
         dto,
         _ct
      );

      // Assert
      response.StatusCode.Should().Be(HttpStatusCode.NotFound);
   }

   [Fact]
   public async Task ReturnLoanAtDeskAsync_ok() {
      // Arrange
      Guid loanId = default;

      await Factory.WithScopeAsync(async sp => {
         var loanRepository = sp.GetRequiredService<ILoanRepository>();
         var unitOfWork = sp.GetRequiredService<IUnitOfWork>();
         var seed = sp.GetRequiredService<TestSeed>();

         var loan = seed.Loan1();
         loanId = loan.Id;

         loanRepository.Add(
            loan: loan
         );

         await unitOfWork.SaveAllChangesAsync(
            "Loan inserted",
            _ct
         );

         unitOfWork.ClearChangeTracker();
      });

      // Act
      var response = await Client.PatchAsync(
         $"{_url}/loans/{loanId}/return-at-desk",
         content: null,
         cancellationToken: _ct
      );

      // Assert
      response.StatusCode.Should().Be(HttpStatusCode.OK);

      var actualLoanDto = await response.Content
         .ReadFromJsonAsync<LoanDto>(
            _ct
         );

      actualLoanDto.Should().NotBeNull();
      actualLoanDto!.Id.Should().Be(loanId);
      actualLoanDto.Status.Should().Be((int)LoanStatus.Returned);
      actualLoanDto.ReturnedAt.Should().NotBeNull();
   }

   [Fact]
   public async Task ReturnLoanAtDeskAsync_unknown_loan_returns_not_found() {
      // Arrange
      var unknownLoanId = Guid.Parse("a1999999-0000-0000-0000-000000000000");

      // Act
      var response = await Client.PatchAsync(
         $"{_url}/loans/{unknownLoanId}/return-at-desk",
         content: null,
         cancellationToken: _ct
      );

      // Assert
      response.StatusCode.Should().Be(HttpStatusCode.NotFound);
   }

   [Fact]
   public async Task ReturnLoanAtDeskAsync_already_returned_loan_returns_conflict() {
      // Arrange
      Guid loanId = default;

      await Factory.WithScopeAsync(async sp => {
         var loanRepository = sp.GetRequiredService<ILoanRepository>();
         var unitOfWork = sp.GetRequiredService<IUnitOfWork>();
         var seed = sp.GetRequiredService<TestSeed>();

         var loan = seed.Loan1();
         loanId = loan.Id;

         loanRepository.Add(
            loan: loan
         );

         await unitOfWork.SaveAllChangesAsync(
            "Loan inserted",
            _ct
         );

         var resultReturned = loan.ReturnAtDesk(
            returnedAt: loan.LoanDate.AddDays(1)
         );

         resultReturned.IsSuccess.Should().BeTrue();

         await unitOfWork.SaveAllChangesAsync(
            "Loan returned",
            _ct
         );

         unitOfWork.ClearChangeTracker();
      });

      // Act
      var response = await Client.PatchAsync(
         $"{_url}/loans/{loanId}/return-at-desk",
         content: null,
         cancellationToken: _ct
      );

      // Assert
      response.StatusCode.Should().Be(HttpStatusCode.Conflict);
   }

   [Fact]
   public async Task RenewLoanAsync_ok() {
      // Arrange
      Guid loanId = default;
      DateTime oldDueDate = default;

      await Factory.WithScopeAsync(async sp => {
         var loanRepository = sp.GetRequiredService<ILoanRepository>();
         var unitOfWork = sp.GetRequiredService<IUnitOfWork>();
         var seed = sp.GetRequiredService<TestSeed>();

         var loan = seed.Loan1();
         loanId = loan.Id;
         oldDueDate = loan.DueDate;

         loanRepository.Add(
            loan: loan
         );

         await unitOfWork.SaveAllChangesAsync(
            "Loan inserted",
            _ct
         );

         unitOfWork.ClearChangeTracker();
      });

      // Act
      var response = await Client.PatchAsync(
         $"{_url}/loans/{loanId}/renew",
         content: null,
         cancellationToken: _ct
      );

      // Assert
      response.StatusCode.Should().Be(HttpStatusCode.OK);

      var actualLoanDto = await response.Content
         .ReadFromJsonAsync<LoanDto>(
            _ct
         );

      actualLoanDto.Should().NotBeNull();
      actualLoanDto!.Id.Should().Be(loanId);
      actualLoanDto.Status.Should().Be((int)LoanStatus.Borrowed);
      actualLoanDto.RenewalCount.Should().Be(1);

      actualLoanDto.DueDate.Should().Be(
         oldDueDate.AddDays(LoanRules.StandardRenewalDays)
      );
   }

   [Fact]
   public async Task RenewLoanAsync_unknown_loan_returns_not_found() {
      // Arrange
      var unknownLoanId = Guid.Parse("a1999999-0000-0000-0000-000000000000");

      // Act
      var response = await Client.PatchAsync(
         $"{_url}/loans/{unknownLoanId}/renew",
         content: null,
         cancellationToken: _ct
      );

      // Assert
      response.StatusCode.Should().Be(HttpStatusCode.NotFound);
   }

   [Fact]
   public async Task RenewLoanAsync_returned_loan_returns_conflict() {
      // Arrange
      Guid loanId = default;

      await Factory.WithScopeAsync(async sp => {
         var loanRepository = sp.GetRequiredService<ILoanRepository>();
         var unitOfWork = sp.GetRequiredService<IUnitOfWork>();
         var seed = sp.GetRequiredService<TestSeed>();

         var loan = seed.Loan1();
         loanId = loan.Id;

         loanRepository.Add(
            loan: loan
         );

         await unitOfWork.SaveAllChangesAsync(
            "Loan inserted",
            _ct
         );

         var resultReturned = loan.ReturnAtDesk(
            returnedAt: loan.LoanDate.AddDays(1)
         );

         resultReturned.IsSuccess.Should().BeTrue();

         await unitOfWork.SaveAllChangesAsync(
            "Loan returned",
            _ct
         );

         unitOfWork.ClearChangeTracker();
      });

      // Act
      var response = await Client.PatchAsync(
         $"{_url}/loans/{loanId}/renew",
         content: null,
         cancellationToken: _ct
      );

      // Assert
      response.StatusCode.Should().Be(HttpStatusCode.Conflict);
   }

   [Fact]
   public async Task RenewLoanAsync_overdue_loan_returns_conflict() {
      // Arrange
      Guid loanId = default;

      await Factory.WithScopeAsync(async sp => {
         var loanRepository = sp.GetRequiredService<ILoanRepository>();
         var unitOfWork = sp.GetRequiredService<IUnitOfWork>();
         var seed = sp.GetRequiredService<TestSeed>();

         var overdueLoan = seed.Loan3();
         loanId = overdueLoan.Id;

         loanRepository.Add(
            loan: overdueLoan
         );

         await unitOfWork.SaveAllChangesAsync(
            "Overdue loan inserted",
            _ct
         );

         unitOfWork.ClearChangeTracker();
      });

      // Act
      var response = await Client.PatchAsync(
         $"{_url}/loans/{loanId}/renew",
         content: null,
         cancellationToken: _ct
      );

      // Assert
      response.StatusCode.Should().Be(HttpStatusCode.Conflict);
   }
}