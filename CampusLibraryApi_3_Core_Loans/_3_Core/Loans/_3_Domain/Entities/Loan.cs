using CampusLibraryApi._2_BuildingBlocks;
using CampusLibraryApi._2_BuildingBlocks._3_Domain.Entities;
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
      if(id == Guid.Empty)
         return Result<Loan>.Failure(LoanErrors.LoanIdRequired);

      if(readerId == Guid.Empty)
         return Result<Loan>.Failure(LoanErrors.ReaderIdRequired);

      if(bookItemId == Guid.Empty)
         return Result<Loan>.Failure(LoanErrors.BookItemIdRequired);

      if(loanPeriodVo is null)
         return Result<Loan>.Failure(LoanErrors.LoanPeriodRequired);

      var loan = new Loan(
         id: id,
         readerId: readerId,
         bookItemId: bookItemId,
         loanPeriodVo: loanPeriodVo
      );

      var resultCreated = loan.Initialize(loanPeriodVo.LoanDate);
      if(resultCreated.IsFailure)
         return Result<Loan>.Failure(resultCreated.Error);

      return Result<Loan>.Success(loan);
   }

   //--- domain methods --------------------------------------------------------
   public Result Renew(
      DateTime utcNow,
      DateTime newDueDate
   ) {
      if(!IsValidUtc(utcNow))
         return Result.Failure(LoanErrors.InvalidUtcNow);

      if(IsOverdue(utcNow))
         return Result.Failure(LoanErrors.LoanAlreadyOverdue);

      if(RenewalCount >= LoanRules.MaxRenewals)
         return Result.Failure(LoanErrors.MaxRenewalsReached);

      Result<LoanPeriodVo> renewedPeriodResult = LoanPeriodVo.RenewUntil(
         newDueDate: newDueDate
      );

      if(renewedPeriodResult.IsFailure)
         return Result.Failure(renewedPeriodResult.Error);

      LoanPeriodVo = renewedPeriodResult.Value;
      RenewalCount += 1;

      Touch(updatedAt: utcNow);

      return Result.Success();
   }

   public bool BelongsToReader(Guid readerId)
      => ReaderId == readerId;

   public bool IsOverdue(DateTime utcNow)
      => DueDate < utcNow;

   public bool CanRenew(DateTime utcNow)
      => RenewalCount < LoanRules.MaxRenewals &&
         !IsOverdue(
            utcNow: utcNow
         );

   private static bool IsValidUtc(DateTime value)
      => value != default &&
         value.Kind == DateTimeKind.Utc;
}

/*
Lernziele und Didaktik
----------------------

Ein Loan repräsentiert ausschließlich eine aktuell bestehende Ausleihe.
Solange ein Loan gespeichert ist, ist das referenzierte BookItem ausgeliehen.
Bei der Rückgabe am Service Desk wird der Loan physisch gelöscht.

Deshalb benötigt Loan weder einen Status noch einen Rückgabezeitpunkt:

- Loan vorhanden: BookItem ist ausgeliehen.
- Loan nicht vorhanden: Es besteht keine Ausleihe für dieses BookItem.

Das Modell vermeidet damit doppelte Zustandsinformationen. Ein Loan kann
nicht gleichzeitig vorhanden und dennoch als zurückgegeben markiert sein.

Die Ausleihdauer wird weiterhin durch LoanPeriodVo geschützt. RenewalCount
und die Renew-Methode modellieren die fachlichen Regeln für Verlängerungen.
*/
