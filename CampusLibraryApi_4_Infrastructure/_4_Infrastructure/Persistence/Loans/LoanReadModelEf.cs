using System.Linq.Expressions;
using CampusLibraryApi._2_BuildingBlocks;
using CampusLibraryApi._2_BuildingBlocks._1_Ports;
using CampusLibraryApi._2_BuildingBlocks._1_Ports.Contracts;
using CampusLibraryApi._3_Core.Loans._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Loans._3_Domain.Entities;
using CampusLibraryApi._3_Core.Loans._3_Domain.Errors;
using CampusLibraryApi._3_Core.Loans._3_Domain.Policies;
using CampusLibraryApi._3_Core.Loans._2_Application.Dtos;
using Microsoft.EntityFrameworkCore;

namespace CampusLibraryApi._4_Infrastructure.Persistence.Loans;

internal sealed class LoanReadModelEf(
   ILoanDbContext loanDbContext,
   IReaderLoanContract readerLoanContract,
   IBookItemLoanContract bookItemLoanContract,
   IClock clock
) : ILoanReadModel {

   public async Task<Result<LoanDto>> FindByIdAsync(
      Guid id,
      CancellationToken ct
   ) {
      if(id == Guid.Empty)
         return Result<LoanDto>.Failure(LoanErrors.InvalidLoanId);

      var loan = await loanDbContext.Loans
         .AsNoTracking()
         .Where(item => item.Id == id)
         .Select(LoanToProjectionDto)
         .FirstOrDefaultAsync(ct);

      return loan is null
         ? Result<LoanDto>.Failure(LoanErrors.LoanNotFound)
         : await ToLoanDtoAsync(loan, ct);
   }

   public async Task<Result<LoanDto>> FindByIdForReaderAsync(
      Guid id,
      Guid readerId,
      CancellationToken ct
   ) {
      if(id == Guid.Empty)
         return Result<LoanDto>.Failure(LoanErrors.InvalidLoanId);
      if(readerId == Guid.Empty)
         return Result<LoanDto>.Failure(LoanErrors.InvalidReaderId);

      var loan = await loanDbContext.Loans
         .AsNoTracking()
         .Where(item => item.Id == id && item.ReaderId == readerId)
         .Select(LoanToProjectionDto)
         .FirstOrDefaultAsync(ct);

      // Do not disclose whether the loan exists for another Reader.
      return loan is null
         ? Result<LoanDto>.Failure(LoanErrors.LoanNotFound)
         : await ToLoanDtoAsync(loan, ct);
   }

   public async Task<Result<IReadOnlyList<LoanDto>>> FindAllBorrowedAsync(
      CancellationToken ct
   ) => await SelectAsync(
      query: loanDbContext.Loans,
      ct: ct
   );

   public async Task<Result<IReadOnlyList<LoanDto>>> FindBorrowedByReaderIdAsync(
      Guid readerId,
      CancellationToken ct
   ) {
      if(readerId == Guid.Empty)
         return Result<IReadOnlyList<LoanDto>>.Failure(LoanErrors.InvalidReaderId);

      return await SelectAsync(
         query: loanDbContext.Loans.Where(item => item.ReaderId == readerId),
         ct: ct
      );
   }

   private async Task<Result<IReadOnlyList<LoanDto>>> SelectAsync(
      IQueryable<Loan> query,
      CancellationToken ct
   ) {
      var loans = await query
         .AsNoTracking()
         .OrderBy(item => item.LoanPeriodVo.DueDate)
         .ThenBy(item => item.LoanPeriodVo.LoanDate)
         .ThenBy(item => item.Id)
         .Select(LoanToProjectionDto)
         .ToListAsync(ct);

      List<LoanDto> dtos = [];
      foreach(var loan in loans) {
         var result = await ToLoanDtoAsync(loan, ct);
         if(result.IsFailure)
            return Result<IReadOnlyList<LoanDto>>.Failure(result.Error);
         dtos.Add(result.Value);
      }

      return Result<IReadOnlyList<LoanDto>>.Success(dtos);
   }

   private async Task<Result<LoanDto>> ToLoanDtoAsync(
      LoanProjectionDto loan,
      CancellationToken ct
   ) {
      var readerResult = await readerLoanContract
         .FindReaderForExistingLoanAsync(loan.ReaderId, ct);
      if(readerResult.IsFailure)
         return Result<LoanDto>.Failure(readerResult.Error);

      var bookItemResult = await bookItemLoanContract
         .FindBookItemForLoanAsync(loan.BookItemId, ct);
      if(bookItemResult.IsFailure)
         return Result<LoanDto>.Failure(bookItemResult.Error);

      var reader = readerResult.Value;
      var bookItem = bookItemResult.Value;
      var utcNow = clock.UtcNow;
      var isOverdue = loan.DueDate < utcNow;

      return Result<LoanDto>.Success(
         new LoanDto(
            Id: loan.Id,
            ReaderId: loan.ReaderId,
            Firstname: reader.Firstname,
            Lastname: reader.Lastname,
            Email: reader.Email,
            BookItemId: loan.BookItemId,
            BookId: bookItem.BookId,
            AuthorsText: bookItem.AuthorsText,
            Title: bookItem.Title,
            Subtitle: bookItem.Subtitle,
            Isbn: bookItem.Isbn,
            BookIsActive: bookItem.BookIsActive,
            IsAvailableForLoan: bookItem.IsAvailableForLoan,
            LoanDate: loan.LoanDate,
            DueDate: loan.DueDate,
            RenewalCount: loan.RenewalCount,
            IsOverdue: isOverdue,
            CanRenew: !isOverdue && loan.RenewalCount < LoanRules.MaxRenewals
         )
      );
   }

   private static readonly Expression<Func<Loan, LoanProjectionDto>> LoanToProjectionDto =
      loan => new LoanProjectionDto(
         loan.Id,
         loan.ReaderId,
         loan.BookItemId,
         loan.LoanPeriodVo.LoanDate,
         loan.LoanPeriodVo.DueDate,
         loan.RenewalCount
      );

   private sealed record LoanProjectionDto(
      Guid Id,
      Guid ReaderId,
      Guid BookItemId,
      DateTime LoanDate,
      DateTime DueDate,
      int RenewalCount
   );
}