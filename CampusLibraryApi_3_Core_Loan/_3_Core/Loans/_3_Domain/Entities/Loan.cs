using CampusLibraryApi._2_BuildingBlocks;
using CampusLibraryApi._2_BuildingBlocks._3_Domain.Entities;
using CampusLibraryApi._3_Core.Loans._3_Domain.Enums;
using CampusLibraryApi._3_Core.Loans._3_Domain.Errors;
using CampusLibraryApi._3_Core.Loans._3_Domain.Policies;
using CampusLibraryApi._3_Core.Loans._3_Domain.ValueObjects;
namespace CampusLibraryApi._3_Core.Loans._3_Domain.Entities;

public sealed class Loan : AggregateRoot {
   //--- properties ------------------------------------------------------------
   // Inherited from Entity / AggregateRoot:
   // public Guid Id { get; protected set; }
   // public DateTime CreatedAt { get; protected set; }
   // public DateTime UpdatedAt { get; protected set; }

   // EF Core sets this property when materializing the aggregate.
   // The domain factory always creates a valid LoanPeriodVo.
   public LoanPeriodVo LoanPeriodVo { get; private set; } = null!;
   public DateTime LoanDate => LoanPeriodVo.LoanDate;
   public DateTime DueDate => LoanPeriodVo.DueDate;
   
   // Actual return timestamp.
   // Null means: the book item has not been returned yet.
   public DateTime? ReturnedAt { get; private set; }

   public LoanStatus Status { get; private set; }
   public int RenewalCount { get; private set; }

   public Guid ReaderId { get; private set; }
   public Guid BookItemId { get; private set; }
   
   //--- constructors ----------------------------------------------------------
   // Required by EF Core.
   private Loan() {
   }

   // Domain ctor.
   private Loan(
      Guid id,
      Guid readerId,
      Guid bookItemId,
      LoanPeriodVo loanPeriodVo
   ) {
      Id = id;
      ReaderId = readerId;
      BookItemId = bookItemId;
      LoanPeriodVo = loanPeriodVo;
      ReturnedAt = null;
      Status = LoanStatus.Active;
      RenewalCount = 0;
   }

   //--- factory methods -------------------------------------------------------
   // Creates a new Loan aggregate and initializes its timestamps.
   // Validation errors are returned as Result failures.
   public static Result<Loan> Create(
      Guid id,
      Guid readerId,
      Guid bookItemId,
      LoanPeriodVo? loanPeriodVo
   ) {
      if (id == Guid.Empty)
         return Result<Loan>.Failure(LoanErrors.LoanIdRequired);

      if (readerId == Guid.Empty)
         return Result<Loan>.Failure(LoanErrors.ReaderIdRequired);

      if (bookItemId == Guid.Empty)
         return Result<Loan>.Failure(LoanErrors.BookItemIdRequired);

      if (loanPeriodVo is null)
         return Result<Loan>.Failure(LoanErrors.LoanPeriodRequired);

      var loan = new Loan(
         id: id,
         readerId: readerId,
         bookItemId: bookItemId,
         loanPeriodVo: loanPeriodVo
      );

      var resultCreated = loan.Initialize(
         createdAt: loanPeriodVo.LoanDate
      );
      if (resultCreated.IsFailure)
         return Result<Loan>.Failure(resultCreated.Error);

      return Result<Loan>.Success(loan);
   }

   //--- domain methods --------------------------------------------------------
   // Returns the book item at the service desk.
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

      Touch(
         updatedAt: returnedAt
      );

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

      if (IsOverdue(
            utcNow: utcNow
         ))
         return Result.Failure(LoanErrors.LoanAlreadyOverdue);

      if (RenewalCount >= LoanRules.MaxRenewals)
         return Result.Failure(LoanErrors.MaxRenewalsReached);

      Result<LoanPeriodVo> renewedPeriodResult = LoanPeriodVo.RenewUntil(
         newDueDate: newDueDate
      );

      if (renewedPeriodResult.IsFailure)
         return Result.Failure(renewedPeriodResult.Error);

      LoanPeriodVo = renewedPeriodResult.Value;
      RenewalCount += 1;

      Touch(
         updatedAt: utcNow
      );

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
         !IsOverdue(
            utcNow: utcNow
         );

   private static bool IsValidUtc(DateTime value)
      => value != default &&
         value.Kind == DateTimeKind.Utc;
}