using CampusLibraryApi._2_BuildingBlocks;
using CampusLibraryApi._2_BuildingBlocks._3_Domain.Entities;
using CampusLibraryApi._3_Core.Loans._3_Domain.Enums;
using CampusLibraryApi._3_Core.Loans._3_Domain.Errors;
using CampusLibraryApi._3_Core.Loans._3_Domain.Policies;
using CampusLibraryApi._3_Core.Loans._3_Domain.ValueObjects;
namespace CampusLibraryApi._3_Core.Loans._3_Domain.Entities;

public sealed class Loan : AggregateRoot {
   public Guid ReaderId { get; private set; }
   public Guid BookItemId { get; private set; }
   public DateTime LoanDate { get; private set; }
   public DateTime DueDate { get; private set; }
   public DateTime? ReturnedAt { get; private set; }
   public LoanStatus Status { get; private set; }
   public int RenewalCount { get; private set; }

   private Loan() {
      // Required by EF Core.
   }

   private Loan(
      Guid id,
      Guid readerId,
      Guid bookItemId,
      DateTime loanDate,
      DateTime dueDate
   ) {
      Id = id;
      ReaderId = readerId;
      BookItemId = bookItemId;
      LoanDate = loanDate;
      DueDate = dueDate;
      ReturnedAt = null;
      Status = LoanStatus.Active;
      RenewalCount = 0;

      Initialize(createdAt: loanDate);
   }

   public static Result<Loan> Create(
      Guid id,
      Guid readerId,
      Guid bookItemId,
      DateTime loanDate,
      DateTime dueDate
   ) {
      if (id == Guid.Empty)
         return Result<Loan>.Failure(LoanErrors.LoanIdRequired);

      if (readerId == Guid.Empty)
         return Result<Loan>.Failure(LoanErrors.ReaderIdRequired);

      if (bookItemId == Guid.Empty)
         return Result<Loan>.Failure(LoanErrors.BookItemIdRequired);

      Result<LoanPeriodVo> periodResult = LoanPeriodVo.Create(
         loanDate: loanDate,
         dueDate: dueDate
      );

      if (!periodResult.IsSuccess)
         return Result<Loan>.Failure(periodResult.Error!);

      Loan loan = new(
         id: id,
         readerId: readerId,
         bookItemId: bookItemId,
         loanDate: periodResult.Value!.LoanDate,
         dueDate: periodResult.Value!.DueDate
      );

      return Result<Loan>.Success(loan);
   }

   public Result ReturnAtDesk(DateTime returnedAt) {
      if (Status == LoanStatus.Returned)
         return Result.Failure(LoanErrors.LoanAlreadyReturned);

      if (Status != LoanStatus.Active)
         return Result.Failure(LoanErrors.LoanNotActive);

      if (!IsValidUtc(returnedAt))
         return Result.Failure(LoanErrors.InvalidReturnedAt);

      if (returnedAt < LoanDate)
         return Result.Failure(LoanErrors.ReturnedAtMustNotBeBeforeLoanDate);

      ReturnedAt = returnedAt;
      Status = LoanStatus.Returned;

      Touch(updatedAt: returnedAt);

      return Result.Success();
   }

   public Result Renew(
      DateTime utcNow,
      DateTime newDueDate
   ) {
      if (Status == LoanStatus.Returned)
         return Result.Failure(LoanErrors.LoanAlreadyReturned);

      if (Status != LoanStatus.Active)
         return Result.Failure(LoanErrors.LoanNotActive);

      if (!IsValidUtc(utcNow))
         return Result.Failure(LoanErrors.InvalidUtcNow);

      if (IsOverdue(utcNow: utcNow))
         return Result.Failure(LoanErrors.LoanAlreadyOverdue);

      if (RenewalCount >= LoanRules.MaxRenewals)
         return Result.Failure(LoanErrors.MaxRenewalsReached);

      LoanPeriodVo currentPeriod = NewPeriodFromCurrentState();

      Result<LoanPeriodVo> renewedPeriodResult = currentPeriod.RenewUntil(
         newDueDate: newDueDate
      );

      if (!renewedPeriodResult.IsSuccess)
         return Result.Failure(renewedPeriodResult.Error!);

      DueDate = renewedPeriodResult.Value!.DueDate;
      RenewalCount += 1;

      Touch(updatedAt: utcNow);

      return Result.Success();
   }

   public bool BelongsToReader(Guid readerId)
      => ReaderId == readerId;

   public bool IsOverdue(DateTime utcNow)
      => Status == LoanStatus.Active &&
         DueDate < utcNow;

   public bool CanRenew(DateTime utcNow)
      => Status == LoanStatus.Active &&
         RenewalCount < LoanRules.MaxRenewals &&
         !IsOverdue(utcNow: utcNow);

   private LoanPeriodVo NewPeriodFromCurrentState()
      => LoanPeriodVo.Create(
            loanDate: LoanDate,
            dueDate: DueDate
         ).Value!;

   private static bool IsValidUtc(DateTime value)
      => value != default &&
         value.Kind == DateTimeKind.Utc;
}
