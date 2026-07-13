using System.Linq.Expressions;
using CampusLibraryApi._2_BuildingBlocks;
using CampusLibraryApi._2_BuildingBlocks._1_Ports;
using CampusLibraryApi._2_BuildingBlocks._1_Ports.Contracts;
using CampusLibraryApi._3_Core.Loans._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Loans._2_Application.Dtos;
using CampusLibraryApi._3_Core.Loans._3_Domain.Entities;
using CampusLibraryApi._3_Core.Loans._3_Domain.Errors;
using CampusLibraryApi._3_Core.Loans._3_Domain.Policies;
using Microsoft.EntityFrameworkCore;

namespace CampusLibraryApi._4_Infrastructure.Persistence.Loans;

internal sealed class LoanReadModelEf(
   ILoanDbContext loanDbContext,
   IReaderLoanContract readerLoanContract,
   IBookItemLoanContract bookItemLoanContract,
   IClock clock
) : ILoanReadModel {

   public async Task<Result<LoanDetailDto>> FindByIdAsync(
      Guid id,
      CancellationToken ct
   ) {
      if(id == Guid.Empty)
         return Result<LoanDetailDto>.Failure(LoanErrors.InvalidLoanId);

      LoanProjectionDto? loan = await loanDbContext.Loans
         .AsNoTracking()
         .Where(loan => loan.Id == id)
         .Select(LoanToProjectionDto)
         .FirstOrDefaultAsync(ct);

      if(loan is null)
         return Result<LoanDetailDto>.Failure(LoanErrors.LoanNotFound);

      return await ToDetailDtoAsync(loan, ct);
   }

   public async Task<Result<LoanDetailDto>> FindByIdForReaderAsync(
      Guid id,
      Guid readerId,
      CancellationToken ct
   ) {
      if(id == Guid.Empty)
         return Result<LoanDetailDto>.Failure(LoanErrors.InvalidLoanId);

      if(readerId == Guid.Empty)
         return Result<LoanDetailDto>.Failure(LoanErrors.InvalidReaderId);

      LoanProjectionDto? loan = await loanDbContext.Loans
         .AsNoTracking()
         .Where(loan => loan.Id == id && loan.ReaderId == readerId)
         .Select(LoanToProjectionDto)
         .FirstOrDefaultAsync(ct);

      // Do not disclose whether the loan exists for another Reader.
      if(loan is null)
         return Result<LoanDetailDto>.Failure(LoanErrors.LoanNotFound);

      return await ToDetailDtoAsync(loan, ct);
   }

   public async Task<Result<IReadOnlyList<LoanListItemDto>>> FindAllBorrowedAsync(
      CancellationToken ct
   ) {
      // Only current loans are stored. Therefore every row represents a
      // borrowed book item and no status filter is required.
      List<LoanProjectionDto> loans = await loanDbContext.Loans
         .AsNoTracking()
         .OrderBy(loan => loan.LoanPeriodVo.DueDate)
         .ThenBy(loan => loan.LoanPeriodVo.LoanDate)
         .ThenBy(loan => loan.Id)
         .Select(LoanToProjectionDto)
         .ToListAsync(ct);

      List<LoanListItemDto> dtos = [];
      foreach(var loan in loans) {
         var resultDto = await ToListItemDtoAsync(loan, ct);
         if(resultDto.IsFailure)
            return Result<IReadOnlyList<LoanListItemDto>>.Failure(resultDto.Error);

         dtos.Add(resultDto.Value);
      }

      return Result<IReadOnlyList<LoanListItemDto>>.Success(dtos);
   }

   public async Task<Result<IReadOnlyList<LoanListItemDto>>> FindBorrowedByReaderIdAsync(
      Guid readerId,
      CancellationToken ct
   ) {
      if(readerId == Guid.Empty)
         return Result<IReadOnlyList<LoanListItemDto>>.Failure(
            LoanErrors.InvalidReaderId
         );

      List<LoanProjectionDto> loans = await loanDbContext.Loans
         .AsNoTracking()
         .Where(loan => loan.ReaderId == readerId)
         .OrderBy(loan => loan.LoanPeriodVo.DueDate)
         .ThenBy(loan => loan.LoanPeriodVo.LoanDate)
         .ThenBy(loan => loan.Id)
         .Select(LoanToProjectionDto)
         .ToListAsync(ct);

      List<LoanListItemDto> dtos = [];
      foreach(LoanProjectionDto loan in loans) {
         Result<LoanListItemDto> resultDto = await ToListItemDtoAsync(
            loan: loan,
            ct: ct
         );

         if(resultDto.IsFailure)
            return Result<IReadOnlyList<LoanListItemDto>>.Failure(
               resultDto.Error
            );

         dtos.Add(resultDto.Value);
      }

      return Result<IReadOnlyList<LoanListItemDto>>.Success(dtos);
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

   private async Task<Result<LoanDetailDto>> ToDetailDtoAsync(
      LoanProjectionDto loan,
      CancellationToken ct
   ) {
      var readerResult = await readerLoanContract
         .FindReaderForExistingLoanAsync(loan.ReaderId, ct);
      if(readerResult.IsFailure)
         return Result<LoanDetailDto>.Failure(readerResult.Error);

      var resultBookItem = await bookItemLoanContract
         .FindBookItemForLoanAsync(loan.BookItemId, ct);
      if(resultBookItem.IsFailure)
         return Result<LoanDetailDto>.Failure(resultBookItem.Error);

      var reader = readerResult.Value;
      var bookItem = resultBookItem.Value;

      return Result<LoanDetailDto>.Success(
         new LoanDetailDto(
            Id: loan.Id,

            ReaderId: loan.ReaderId,
            Firstname: reader.Firstname,
            Lastname: reader.Lastname,
            Email: reader.Email,

            BookItemId: loan.BookItemId,
            BookId: bookItem.BookId,

            Title: bookItem.Title,
            Subtitle: bookItem.Subtitle,
            AuthorsText: bookItem.AuthorsText,
            Isbn: bookItem.Isbn,

            BookIsActive: bookItem.BookIsActive,
            IsAvailableForLoan: bookItem.IsAvailableForLoan,

            LoanDate: loan.LoanDate,
            DueDate: loan.DueDate,
            RenewalCount: loan.RenewalCount,

            IsOverdue: IsOverdue(
               loan: loan,
               utcNow: clock.UtcNow
            ),
            CanRenew: CanRenew(
               loan: loan,
               utcNow: clock.UtcNow
            )
         )
      );
   }

   private async Task<Result<LoanListItemDto>> ToListItemDtoAsync(
      LoanProjectionDto loan,
      CancellationToken ct
   ) {
      var readerResult = await readerLoanContract.FindReaderForExistingLoanAsync(
         readerId: loan.ReaderId,
         ct: ct
      );

      if(readerResult.IsFailure)
         return Result<LoanListItemDto>.Failure(readerResult.Error);

      var bookItemResult = await bookItemLoanContract.FindBookItemForLoanAsync(
         id: loan.BookItemId,
         ct: ct
      );

      if(bookItemResult.IsFailure)
         return Result<LoanListItemDto>.Failure(bookItemResult.Error);

      var reader = readerResult.Value;
      var bookItem = bookItemResult.Value;

      return Result<LoanListItemDto>.Success(
         new LoanListItemDto(
            Id: loan.Id,

            ReaderId: loan.ReaderId,
            Firstname: reader.Firstname,
            Lastname: reader.Lastname,

            BookItemId: loan.BookItemId,

            Title: bookItem.Title,
            Subtitle: bookItem.Subtitle,

            LoanDate: loan.LoanDate,
            DueDate: loan.DueDate,

            IsOverdue: IsOverdue(
               loan: loan,
               utcNow: clock.UtcNow
            )
         )
      );
   }

   private static bool IsOverdue(
      LoanProjectionDto loan,
      DateTime utcNow
   ) => loan.DueDate < utcNow;

   private static bool CanRenew(
      LoanProjectionDto loan,
      DateTime utcNow
   ) => !IsOverdue(
         loan: loan,
         utcNow: utcNow
      ) &&
      loan.RenewalCount < LoanRules.MaxRenewals;

   private sealed record LoanProjectionDto(
      Guid Id,
      Guid ReaderId,
      Guid BookItemId,
      DateTime LoanDate,
      DateTime DueDate,
      int RenewalCount
   );
}
