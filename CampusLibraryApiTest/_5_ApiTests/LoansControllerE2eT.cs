using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;
using CampusLibraryApi._2_BuildingBlocks._1_Ports;
using CampusLibraryApi._3_Core.Catalog._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Loans._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Loans._2_Application.Dtos;
using CampusLibraryApi._3_Core.Loans._2_Application.Mappings;
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
   public async Task GetByIdAsync_ok() {
      // Arrange
      LoanDto expectedLoanDto = default!;

      await Factory.WithScopeAsync(async sp => {
         var repository = sp.GetRequiredService<ILoanRepository>();
         var unitOfWork = sp.GetRequiredService<IUnitOfWork>();
         var seed = sp.GetRequiredService<TestSeed>();

         var loan = seed.Loan1();
         expectedLoanDto = loan.ToLoanDto();

         repository.Add(
            loan: loan
         );

         await unitOfWork.SaveAllChangesAsync(
            "Loan1 inserted",
            _ct
         );

         unitOfWork.ClearChangeTracker();
      });

      // Act
      var response = await Client.GetAsync(
         $"{_url}/loans/{expectedLoanDto.Id}",
         _ct
      );

      var actualLoanDto = await response.Content
         .ReadFromJsonAsync<LoanDto>(
            _ct
         );

      // Assert
      response.StatusCode.Should().Be(HttpStatusCode.OK);
      actualLoanDto.Should().NotBeNull();
      actualLoanDto.Should().BeEquivalentTo(expectedLoanDto);
   }

   [Fact]
   public async Task GetByIdAsync_unknown_id_returns_not_found() {
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
   public async Task GetAllActiveAsync_ok() {
      // Arrange
      List<LoanDto> expectedLoanDtos = [];

      await Factory.WithScopeAsync(async sp => {
         var repository = sp.GetRequiredService<ILoanRepository>();
         var unitOfWork = sp.GetRequiredService<IUnitOfWork>();
         var seed = sp.GetRequiredService<TestSeed>();

         var loans = seed.Loans;

         expectedLoanDtos = loans
            .Where(l => l.Status == LoanStatus.Active)
            .OrderBy(l => l.DueDate)
            .Select(l => l.ToLoanDto())
            .ToList();

         repository.AddRange(
            loans: loans
         );

         await unitOfWork.SaveAllChangesAsync(
            "Loans inserted",
            _ct
         );

         unitOfWork.ClearChangeTracker();
      });

      // Act
      var response = await Client.GetAsync(
         $"{_url}/loans/active",
         _ct
      );

      var actualLoanDtos = await response.Content
         .ReadFromJsonAsync<List<LoanDto>>(
            _ct
         );

      // Assert
      response.StatusCode.Should().Be(HttpStatusCode.OK);
      actualLoanDtos.Should().NotBeNull();

      actualLoanDtos!
         .Should()
         .BeEquivalentTo(
            expectedLoanDtos,
            options => options.WithStrictOrdering()
         );
   }

   [Fact]
   public async Task GetActiveByReaderIdAsync_ok() {
      // Arrange
      Guid readerId = default;
      List<LoanDto> expectedLoanDtos = [];

      await Factory.WithScopeAsync(async sp => {
         var repository = sp.GetRequiredService<ILoanRepository>();
         var unitOfWork = sp.GetRequiredService<IUnitOfWork>();
         var seed = sp.GetRequiredService<TestSeed>();

         var loans = seed.Loans;
         var loan2 = loans[1];
         readerId = loan2.ReaderId;

         expectedLoanDtos = loans
            .Where(l =>
               l.ReaderId == readerId &&
               l.Status == LoanStatus.Active)
            .OrderBy(l => l.DueDate)
            .Select(l => l.ToLoanDto())
            .ToList();

         repository.AddRange(
            loans: loans
         );

         await unitOfWork.SaveAllChangesAsync(
            "Loans inserted",
            _ct
         );

         unitOfWork.ClearChangeTracker();
      });

      // Act
      var response = await Client.GetAsync(
         $"{_url}/readers/{readerId}/loans/active",
         _ct
      );

      var actualLoanDtos = await response.Content
         .ReadFromJsonAsync<List<LoanDto>>(
            _ct
         );

      // Assert
      response.StatusCode.Should().Be(HttpStatusCode.OK);
      actualLoanDtos.Should().NotBeNull();

      actualLoanDtos!
         .Should()
         .BeEquivalentTo(
            expectedLoanDtos,
            options => options.WithStrictOrdering()
         );
   }

   [Fact]
   public async Task GetAllOverdueAsync_ok() {
      // Arrange
      Guid expectedOverdueLoanId = default;

      await Factory.WithScopeAsync(async sp => {
         var repository = sp.GetRequiredService<ILoanRepository>();
         var unitOfWork = sp.GetRequiredService<IUnitOfWork>();
         var seed = sp.GetRequiredService<TestSeed>();

         var loans = seed.Loans;
         var overdueLoan = loans[2];
         expectedOverdueLoanId = overdueLoan.Id;

         repository.AddRange(
            loans: loans
         );

         await unitOfWork.SaveAllChangesAsync(
            "Loans inserted",
            _ct
         );

         unitOfWork.ClearChangeTracker();
      });

      // Act
      var response = await Client.GetAsync(
         $"{_url}/loans/overdue",
         _ct
      );

      var actualLoanDtos = await response.Content
         .ReadFromJsonAsync<List<LoanDto>>(
            _ct
         );

      // Assert
      response.StatusCode.Should().Be(HttpStatusCode.OK);
      actualLoanDtos.Should().NotBeNull();

      actualLoanDtos!
         .Should()
         .ContainSingle(l => l.Id == expectedOverdueLoanId);

      actualLoanDtos!
         .Should()
         .OnlyContain(l => l.Status == LoanStatus.Active);
   }

   [Fact]
   public async Task BorrowAsync_ok() {
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
            bi.Id == Guid.Parse(seed.BookItem1Id));

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

      var actualLoanDto = await response.Content
         .ReadFromJsonAsync<LoanDto>(
            _ct
         );

      // Assert
      response.StatusCode.Should().Be(HttpStatusCode.Created);
      response.Headers.Location.Should().NotBeNull();

      actualLoanDto.Should().NotBeNull();
      actualLoanDto!.Id.Should().Be(Guid.Parse(dto.Id!));
      actualLoanDto.ReaderId.Should().Be(expectedReaderId);
      actualLoanDto.BookItemId.Should().Be(expectedBookItemId);
      actualLoanDto.ReturnedAt.Should().BeNull();
      actualLoanDto.Status.Should().Be(LoanStatus.Active);
      actualLoanDto.RenewalCount.Should().Be(0);

      actualLoanDto.DueDate.Should().Be(
         actualLoanDto.LoanDate.AddDays(LoanRules.StandardLoanDays)
      );
   }

   [Fact]
   public async Task BorrowAsync_book_item_already_borrowed_returns_conflict() {
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
            "Readers, books and loan inserted",
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
   public async Task BorrowAsync_unknown_reader_returns_not_found() {
      // Arrange
      LoanCreateDto dto = default!;

      await Factory.WithScopeAsync(async sp => {
         var bookRepository = sp.GetRequiredService<IBookRepository>();
         var unitOfWork = sp.GetRequiredService<IUnitOfWork>();
         var seed = sp.GetRequiredService<TestSeed>();

         var books = seed.Books;
         var book1 = books[0];

         var bookItem1 = book1.BookItems.Single(bi =>
            bi.Id == Guid.Parse(seed.BookItem1Id));

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
   public async Task ReturnAtDeskAsync_ok() {
      // Arrange
      Guid loanId = default;

      await Factory.WithScopeAsync(async sp => {
         var repository = sp.GetRequiredService<ILoanRepository>();
         var unitOfWork = sp.GetRequiredService<IUnitOfWork>();
         var seed = sp.GetRequiredService<TestSeed>();

         var loan = seed.Loan1();
         loanId = loan.Id;

         repository.Add(
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

      var actualLoanDto = await response.Content
         .ReadFromJsonAsync<LoanDto>(
            _ct
         );

      // Assert
      response.StatusCode.Should().Be(HttpStatusCode.OK);
      actualLoanDto.Should().NotBeNull();
      actualLoanDto!.Id.Should().Be(loanId);
      actualLoanDto.Status.Should().Be(LoanStatus.Returned);
      actualLoanDto.ReturnedAt.Should().NotBeNull();
   }

   [Fact]
   public async Task ReturnAtDeskAsync_unknown_loan_returns_not_found() {
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
   public async Task ReturnAtDeskAsync_already_returned_loan_returns_conflict() {
      // Arrange
      Guid loanId = default;

      await Factory.WithScopeAsync(async sp => {
         var repository = sp.GetRequiredService<ILoanRepository>();
         var unitOfWork = sp.GetRequiredService<IUnitOfWork>();
         var seed = sp.GetRequiredService<TestSeed>();

         var loan = seed.Loan1();
         loanId = loan.Id;

         repository.Add(
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
   public async Task RenewAsync_ok() {
      // Arrange
      Guid loanId = default;
      DateTime oldDueDate = default;

      await Factory.WithScopeAsync(async sp => {
         var repository = sp.GetRequiredService<ILoanRepository>();
         var unitOfWork = sp.GetRequiredService<IUnitOfWork>();
         var seed = sp.GetRequiredService<TestSeed>();

         var loan = seed.Loan1();
         loanId = loan.Id;
         oldDueDate = loan.DueDate;

         repository.Add(
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

      var actualLoanDto = await response.Content
         .ReadFromJsonAsync<LoanDto>(
            _ct
         );

      // Assert
      response.StatusCode.Should().Be(HttpStatusCode.OK);
      actualLoanDto.Should().NotBeNull();
      actualLoanDto!.Id.Should().Be(loanId);
      actualLoanDto.Status.Should().Be(LoanStatus.Active);
      actualLoanDto.RenewalCount.Should().Be(1);

      actualLoanDto.DueDate.Should().Be(
         oldDueDate.AddDays(LoanRules.StandardRenewalDays)
      );
   }

   [Fact]
   public async Task RenewAsync_unknown_loan_returns_not_found() {
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
   public async Task RenewAsync_overdue_loan_returns_conflict() {
      // Arrange
      Guid loanId = default;

      await Factory.WithScopeAsync(async sp => {
         var repository = sp.GetRequiredService<ILoanRepository>();
         var unitOfWork = sp.GetRequiredService<IUnitOfWork>();
         var seed = sp.GetRequiredService<TestSeed>();

         var overdueLoan = seed.Loan3();
         loanId = overdueLoan.Id;

         repository.Add(
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