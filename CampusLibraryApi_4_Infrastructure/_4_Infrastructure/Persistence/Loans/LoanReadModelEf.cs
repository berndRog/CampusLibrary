using System.Linq.Expressions;
using CampusLibraryApi._2_BuildingBlocks;
using CampusLibraryApi._2_BuildingBlocks._1_Ports;
using CampusLibraryApi._2_BuildingBlocks._1_Ports.Contracts;
using CampusLibraryApi._3_Core.Loans._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Loans._2_Application.Dtos;
using CampusLibraryApi._3_Core.Loans._3_Domain.Entities;
using CampusLibraryApi._3_Core.Loans._3_Domain.Enums;
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

   public async Task<Result<IReadOnlyList<LoanListItemDto>>> FindAllBorrowedAsync(
      CancellationToken ct
   ) {
      List<LoanProjectionDto> loans = await loanDbContext.Loans
         .AsNoTracking()
         .Where(loan =>
            loan.Status == LoanStatus.Borrowed &&
            loan.ReturnedAt == null
         )
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

   private static readonly Expression<Func<Loan, LoanProjectionDto>> LoanToProjectionDto =
      loan => new LoanProjectionDto(
         // Id:
         loan.Id,
         // ReaderId:
         loan.ReaderId,
         // BookItemId:
         loan.BookItemId,
         // LoanDate:
         loan.LoanPeriodVo.LoanDate,
         // DueDate:
         loan.LoanPeriodVo.DueDate,
         // ReturnedAt:
         loan.ReturnedAt,
         // Status:
         (int)loan.Status,
         // RenewalCount:
         loan.RenewalCount
      );

   private async Task<Result<LoanDetailDto>> ToDetailDtoAsync(
      LoanProjectionDto loan,
      CancellationToken ct
   ) {
      var readerResult = await readerLoanContract
         .FindReaderForLoanAsync(loan.ReaderId, ct);
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

            BookItemId: loan.BookItemId,
            BookId: bookItem.BookId,
            InventoryNumber: bookItem.InventoryNumber,

            Title: bookItem.Title,
            Subtitle: bookItem.Subtitle,
            AuthorsText: bookItem.AuthorsText,
            Isbn: bookItem.Isbn,

            BookIsActive: bookItem.BookIsActive,
            IsAvailableForLoan: bookItem.IsAvailableForLoan,

            LoanDate: loan.LoanDate,
            DueDate: loan.DueDate,
            ReturnedAt: loan.ReturnedAt,

            Status: loan.Status,
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
      var readerResult = await readerLoanContract.FindReaderForLoanAsync(
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
            InventoryNumber: bookItem.InventoryNumber,

            Title: bookItem.Title,
            Subtitle: bookItem.Subtitle,

            LoanDate: loan.LoanDate,
            DueDate: loan.DueDate,

            Status: loan.Status,
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
   ) => loan.ReturnedAt is null && loan.DueDate < utcNow;

   private static bool CanRenew(
      LoanProjectionDto loan,
      DateTime utcNow
   ) => loan.Status == (int)LoanStatus.Borrowed &&
      loan.ReturnedAt is null &&
      !IsOverdue(
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
      DateTime? ReturnedAt,
      int Status,
      int RenewalCount
   );
}