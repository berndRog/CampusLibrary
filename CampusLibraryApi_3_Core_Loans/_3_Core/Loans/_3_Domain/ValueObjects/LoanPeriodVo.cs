using CampusLibraryApi._2_BuildingBlocks;
using CampusLibraryApi._3_Core.Loans._3_Domain.Errors;
namespace CampusLibraryApi._3_Core.Loans._3_Domain.ValueObjects;

public sealed record LoanPeriodVo {
   public DateTime LoanDate { get; }
   public DateTime DueDate { get; }

   private LoanPeriodVo(
      DateTime loanDate,
      DateTime dueDate
   ) {
      LoanDate = loanDate;
      DueDate = dueDate;
   }

   public static Result<LoanPeriodVo> Create(
      DateTime loanDate,
      DateTime dueDate
   ) {
      if (!IsValidUtc(loanDate))
         return Result<LoanPeriodVo>.Failure(LoanErrors.InvalidLoanDate);

      if (!IsValidUtc(dueDate))
         return Result<LoanPeriodVo>.Failure(LoanErrors.InvalidDueDate);

      if (dueDate <= loanDate)
         return Result<LoanPeriodVo>.Failure(LoanErrors.DueDateMustBeAfterLoanDate);

      LoanPeriodVo period = new(
         loanDate: loanDate,
         dueDate: dueDate
      );

      return Result<LoanPeriodVo>.Success(period);
   }

   public Result<LoanPeriodVo> RenewUntil(DateTime newDueDate) {
      if (!IsValidUtc(newDueDate))
         return Result<LoanPeriodVo>.Failure(LoanErrors.InvalidDueDate);

      if (newDueDate <= DueDate)
         return Result<LoanPeriodVo>.Failure(LoanErrors.NewDueDateMustBeAfterCurrentDueDate);

      LoanPeriodVo period = new(
         loanDate: LoanDate,
         dueDate: newDueDate
      );

      return Result<LoanPeriodVo>.Success(period);
   }

   private static bool IsValidUtc(DateTime value)
      => value != default &&
         value.Kind == DateTimeKind.Utc;
}