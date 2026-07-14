using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;
using CampusLibraryApi._2_BuildingBlocks._1_Ports;
using CampusLibraryApi._3_Core.Catalog._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Loans._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Loans._2_Application.Dtos;
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
   public async Task GetMyBorrowedLoansAsync_returns_only_current_reader_loans() {
      Guid expectedLoanId = default;
      string subject = string.Empty;
      string username = string.Empty;

      await Factory.WithScopeAsync(async sp => {
         var readerRepository = sp.GetRequiredService<IReaderRepository>();
         var bookRepository = sp.GetRequiredService<IBookRepository>();
         var loanRepository = sp.GetRequiredService<ILoanRepository>();
         var unitOfWork = sp.GetRequiredService<IUnitOfWork>();
         var seed = sp.GetRequiredService<TestSeed>();

         var reader = seed.Reader1();
         var loan = seed.Loan1();

         expectedLoanId = loan.Id;
         subject = reader.Subject;
         username = reader.EmailVo.Value;

         readerRepository.AddRange(seed.Readers);
         bookRepository.AddRange(seed.Books);
         loanRepository.AddRange(seed.Loans);

         await unitOfWork.SaveAllChangesAsync(
            "Readers, books and loans inserted",
            _ct
         );

         unitOfWork.ClearChangeTracker();
      });

      using var request = CreateReaderRequest(
         method: HttpMethod.Get,
         url: $"{_url}/loans/me",
         subject: subject,
         username: username
      );

      var response = await Client.SendAsync(
         request: request,
         cancellationToken: _ct
      );

      var body = await response.Content.ReadAsStringAsync(_ct);

      response.StatusCode.Should().Be(
         expected: HttpStatusCode.OK,
         because: body
      );

      var loans = await response.Content.ReadFromJsonAsync<List<LoanDto>>(
         _ct
      );

      loans.Should().NotBeNull();
      loans.Should().ContainSingle();
      loans![0].Id.Should().Be(expectedLoanId);
   }

   [Fact]
   public async Task GetMyLoanByIdAsync_loan_of_other_reader_returns_not_found() {
      Guid otherLoanId = default;
      string subject = string.Empty;
      string username = string.Empty;

      await Factory.WithScopeAsync(async sp => {
         var readerRepository = sp.GetRequiredService<IReaderRepository>();
         var bookRepository = sp.GetRequiredService<IBookRepository>();
         var loanRepository = sp.GetRequiredService<ILoanRepository>();
         var unitOfWork = sp.GetRequiredService<IUnitOfWork>();
         var seed = sp.GetRequiredService<TestSeed>();

         var currentReader = seed.Reader1();
         var otherLoan = seed.Loan2();

         otherLoanId = otherLoan.Id;
         subject = currentReader.Subject;
         username = currentReader.EmailVo.Value;

         readerRepository.AddRange(seed.Readers);
         bookRepository.AddRange(seed.Books);
         loanRepository.AddRange(seed.Loans);

         await unitOfWork.SaveAllChangesAsync(
            "Readers, books and loans inserted",
            _ct
         );

         unitOfWork.ClearChangeTracker();
      });

      using var request = CreateReaderRequest(
         method: HttpMethod.Get,
         url: $"{_url}/loans/me/{otherLoanId}",
         subject: subject,
         username: username
      );

      var response = await Client.SendAsync(
         request: request,
         cancellationToken: _ct
      );

      response.StatusCode.Should().Be(HttpStatusCode.NotFound);
   }

   [Fact]
   public async Task BorrowMyBookItemAsync_uses_reader_from_token() {
      Guid expectedReaderId = default;
      Guid bookItemId = default;
      string subject = string.Empty;
      string username = string.Empty;

      await Factory.WithScopeAsync(async sp => {
         var readerRepository = sp.GetRequiredService<IReaderRepository>();
         var bookRepository = sp.GetRequiredService<IBookRepository>();
         var unitOfWork = sp.GetRequiredService<IUnitOfWork>();
         var seed = sp.GetRequiredService<TestSeed>();

         var reader = seed.Reader1();
         var books = seed.Books;

         expectedReaderId = reader.Id;
         subject = reader.Subject;
         username = reader.EmailVo.Value;
         bookItemId = books
            .SelectMany(book => book.BookItems)
            .First()
            .Id;

         readerRepository.AddRange(seed.Readers);
         bookRepository.AddRange(books);

         await unitOfWork.SaveAllChangesAsync(
            "Readers and books inserted",
            _ct
         );

         unitOfWork.ClearChangeTracker();
      });

      using var request = CreateReaderRequest(
         method: HttpMethod.Post,
         url: $"{_url}/loans/me",
         subject: subject,
         username: username
      );
      request.Content = JsonContent.Create(
         inputValue: new LoanBorrowMeDto(
            BookItemId: bookItemId
         )
      );

      var response = await Client.SendAsync(
         request: request,
         cancellationToken: _ct
      );

      var body = await response.Content.ReadAsStringAsync(_ct);

      response.StatusCode.Should().Be(
         expected: HttpStatusCode.Created,
         because: body
      );

      var loan = await response.Content.ReadFromJsonAsync<LoanDto>(_ct);

      loan.Should().NotBeNull();
      loan!.ReaderId.Should().Be(expectedReaderId);
      loan.BookItemId.Should().Be(bookItemId);
   }

   [Fact]
   public async Task RenewMyLoanAsync_renews_only_current_reader_loan() {
      Guid loanId = default;
      DateTime oldDueDate = default;
      string subject = string.Empty;
      string username = string.Empty;

      await Factory.WithScopeAsync(async sp => {
         var readerRepository = sp.GetRequiredService<IReaderRepository>();
         var bookRepository = sp.GetRequiredService<IBookRepository>();
         var loanRepository = sp.GetRequiredService<ILoanRepository>();
         var unitOfWork = sp.GetRequiredService<IUnitOfWork>();
         var seed = sp.GetRequiredService<TestSeed>();

         var reader = seed.Reader1();
         var loan = seed.Loan1();

         loanId = loan.Id;
         oldDueDate = loan.DueDate;
         subject = reader.Subject;
         username = reader.EmailVo.Value;

         readerRepository.AddRange(seed.Readers);
         bookRepository.AddRange(seed.Books);
         loanRepository.Add(loan);

         await unitOfWork.SaveAllChangesAsync(
            "Readers, books and loan inserted",
            _ct
         );

         unitOfWork.ClearChangeTracker();
      });

      using var request = CreateReaderRequest(
         method: HttpMethod.Patch,
         url: $"{_url}/loans/me/{loanId}/renew",
         subject: subject,
         username: username
      );

      var response = await Client.SendAsync(
         request: request,
         cancellationToken: _ct
      );

      var body = await response.Content.ReadAsStringAsync(_ct);

      response.StatusCode.Should().Be(
         expected: HttpStatusCode.OK,
         because: body
      );

      var loan = await response.Content.ReadFromJsonAsync<LoanDto>(_ct);

      loan.Should().NotBeNull();
      loan!.RenewalCount.Should().Be(1);
      loan.DueDate.Should().Be(
         oldDueDate.AddDays(LoanRules.StandardRenewalDays)
      );
   }

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
         .ReadFromJsonAsync<LoanDto>(
            _ct
         );

      actualLoanDto.Should().NotBeNull();

      actualLoanDto!.Id.Should().Be(loanId);
      actualLoanDto.ReaderId.Should().Be(readerId);
      actualLoanDto.BookItemId.Should().Be(bookItemId);

      actualLoanDto.Firstname.Should().NotBeNullOrWhiteSpace();
      actualLoanDto.Lastname.Should().NotBeNullOrWhiteSpace();
      actualLoanDto.Title.Should().NotBeNullOrWhiteSpace();

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
         .ReadFromJsonAsync<List<LoanDto>>(
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
         !string.IsNullOrWhiteSpace(l.Firstname) &&
         !string.IsNullOrWhiteSpace(l.Lastname) &&
         !string.IsNullOrWhiteSpace(l.Title) 
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
   public async Task BorrowBookItemAsync_same_book_different_item_already_borrowed_by_reader_returns_conflict() {
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
         var book1 = books[0];

         var secondBookItemOfSameBook = book1.BookItems.Single(bookItem =>
            bookItem.Id == Guid.Parse(seed.BookItem2Id)
         );

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
            Id: "a1000010-0000-0000-0000-000000000000",
            ReaderId: existingLoan.ReaderId,
            BookItemId: secondBookItemOfSameBook.Id
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
      response.StatusCode.Should().Be(HttpStatusCode.NoContent);

      await Factory.WithScopeAsync(async sp => {
         var loanRepository = sp.GetRequiredService<ILoanRepository>();
         var unitOfWork = sp.GetRequiredService<IUnitOfWork>();

         unitOfWork.ClearChangeTracker();

         var deletedLoan = await loanRepository.FindByIdAsync(
            id: loanId,
            ct: _ct
         );

         deletedLoan.Should().BeNull();
      });
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
   private static HttpRequestMessage CreateReaderRequest(
      HttpMethod method,
      string url,
      string subject,
      string username
   ) {
      var request = new HttpRequestMessage(
         method: method,
         requestUri: url
      );

      request.Headers.Add(
         TestAuthHandler.RolesHeader,
         "Reader"
      );
      request.Headers.Add(
         TestAuthHandler.SubjectHeader,
         subject
      );
      request.Headers.Add(
         TestAuthHandler.UsernameHeader,
         username
      );
      request.Headers.Add(
         TestAuthHandler.CreatedAtHeader,
         "2025-01-01T00:00:00Z"
      );

      return request;
   }

}
