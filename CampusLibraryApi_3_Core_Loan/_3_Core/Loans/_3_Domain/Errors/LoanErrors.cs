using CampusLibraryApi._2_BuildingBlocks._3_Domain.Enums;
using CampusLibraryApi._2_BuildingBlocks._3_Domain.Errors;
namespace CampusLibraryApi._3_Core.Loans._3_Domain.Errors;

public static class LoanErrors {

   // Loan aggregate
   // ------------------------------------------------------------------------
   public static readonly DomainError LoanIdRequired =
      new(
         WebErrorStatus.BadRequest,
         "Loan: Loan Id Required",
         "The loan id is required."
      );

   public static readonly DomainError InvalidLoanId =
      new(
         WebErrorStatus.BadRequest,
         "Loan: Invalid Loan Id",
         "The given loan id is invalid."
      );

   public static readonly DomainError LoanNotFound =
      new(
         WebErrorStatus.NotFound,
         "Loan: Loan Not Found",
         "The loan was not found."
      );

   public static readonly DomainError ReaderIdRequired =
      new(
         WebErrorStatus.BadRequest,
         "Loan: Reader Id Required",
         "The reader id is required."
      );
   
   public static readonly DomainError InvalidReaderId =
      new(
         WebErrorStatus.BadRequest,
         "Loan: Invalid Reader Id",
         "The given reader id is invalid."
      );

   public static readonly DomainError BookItemIdRequired =
      new(
         WebErrorStatus.BadRequest,
         "Loan: Book Item Id Required",
         "The book item id is required."
      );
   
   public static readonly DomainError InvalidBookItemId =
      new(
         WebErrorStatus.BadRequest,
         "Loan: Invalid Book Item Id",
         "The given book item id is invalid."
      );

   public static readonly DomainError InvalidLoanDate =
      new(
         WebErrorStatus.BadRequest,
         "Loan: Invalid Loan Date",
         "The loan date must be a non-default UTC timestamp."
      );

   public static readonly DomainError LoanPeriodRequired = 
      new(
         WebErrorStatus.BadRequest,
         "Loan: LoanPeriod Required",
         "Loan period is required."
      );
   
   public static readonly DomainError InvalidDueDate =
      new(
         WebErrorStatus.BadRequest,
         "Loan: Invalid Due Date",
         "The due date must be a non-default UTC timestamp."
      );

   public static readonly DomainError DueDateMustBeAfterLoanDate =
      new(
         WebErrorStatus.BadRequest,
         "Loan: Due Date Must Be After Loan Date",
         "The due date must be after the loan date."
      );

   public static readonly DomainError InvalidReturnedAt =
      new(
         WebErrorStatus.BadRequest,
         "Loan: Invalid Returned At",
         "ReturnedAt must be a non-default UTC timestamp."
      );

   public static readonly DomainError ReturnedAtMustNotBeBeforeLoanDate =
      new(
         WebErrorStatus.BadRequest,
         "Loan: Returned At Before Loan Date",
         "ReturnedAt must not be before the loan date."
      );

   public static readonly DomainError LoanNotActive =
      new(
         WebErrorStatus.Conflict,
         "Loan: Loan Not Active",
         "The loan must be active."
      );

   public static readonly DomainError LoanAlreadyReturned =
      new(
         WebErrorStatus.Conflict,
         "Loan: Loan Already Returned",
         "The loan has already been returned."
      );

   public static readonly DomainError LoanDoesNotBelongToReader =
      new(
         WebErrorStatus.BadRequest,
         "Loan: Loan Does Not Belong To Reader",
         "The loan does not belong to the current reader."
      );

   public static readonly DomainError LoanAlreadyOverdue =
      new(
         WebErrorStatus.Conflict,
         "Loan: Loan Already Overdue",
         "Overdue loans cannot be renewed by the reader."
      );

   public static readonly DomainError MaxRenewalsReached =
      new(
         WebErrorStatus.Conflict,
         "Loan: Max Renewals Reached",
         "The maximum number of renewals has been reached."
      );

   public static readonly DomainError InvalidUtcNow =
      new(
         WebErrorStatus.BadRequest,
         "Loan: Invalid Current Timestamp",
         "The current timestamp must be a non-default UTC timestamp."
      );


   // Loan period value object
   // ------------------------------------------------------------------------
   public static readonly DomainError NewDueDateMustBeAfterCurrentDueDate =
      new(
         WebErrorStatus.BadRequest,
         "LoanPeriod: New Due Date Must Be Later",
         "The new due date must be after the current due date."
      );


   // Reader reference
   // ------------------------------------------------------------------------
   public static readonly DomainError ReaderNotFound =
      new(
         WebErrorStatus.NotFound,
         "Loan: Reader Not Found",
         "The reader was not found."
      );

   public static readonly DomainError ReaderNotActive =
      new(
         WebErrorStatus.Conflict,
         "Loan: Reader Not Active",
         "The reader must be active."
      );


   // BookItem reference
   // ------------------------------------------------------------------------
   public static readonly DomainError BookItemNotFound =
      new(
         WebErrorStatus.NotFound,
         "Loan: Book Item Not Found",
         "The book item was not found."
      );

   public static readonly DomainError BookItemNotAvailable =
      new(
         WebErrorStatus.Conflict,
         "Loan: Book Item Not Available",
         "The book item must be available."
      );

   public static readonly DomainError BookItemAlreadyBorrowed =
      new(
         WebErrorStatus.Conflict,
         "Loan: Book Item Already Borrowed",
         "The book item is already borrowed."
      );


   // Loan DTOs
   // ------------------------------------------------------------------------
   public static readonly DomainError LoanCreateDtoRequired =
      new(
         WebErrorStatus.BadRequest,
         "Loan: LoanCreateDtoRequired",
         "A LoanCreateDto object must be provided."
      );

   public static readonly DomainError LoanReturnDtoRequired =
      new(
         WebErrorStatus.BadRequest,
         "Loan: LoanReturnDtoRequired",
         "A LoanReturnDto object must be provided."
      );

   public static readonly DomainError LoanRenewDtoRequired =
      new(
         WebErrorStatus.BadRequest,
         "Loan: LoanRenewDtoRequired",
         "A LoanRenewDto object must be provided."
      );
   
   

}
