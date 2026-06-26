using CampusLibraryApi._3_Core.Loans._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Loans._3_Domain.Entities;
using CampusLibraryApi._3_Core.Loans._3_Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CampusLibraryApi._4_Infrastructure.Persistence.Loans;

internal sealed class LoanRepositoryEf(
   ILoanDbContext loanDbContext
) : ILoanRepository {

   public async Task<Loan?> FindByIdAsync(
      Guid id,
      CancellationToken ct
   ) => await loanDbContext.Loans
      .FirstOrDefaultAsync(
         loan => loan.Id == id,
         ct
      );

   public async Task<Loan?> FindBorrowedByBookItemIdAsync(
      Guid bookItemId,
      CancellationToken ct
   ) => await loanDbContext.Loans
      .FirstOrDefaultAsync(
         loan =>
            loan.BookItemId == bookItemId &&
            loan.Status == LoanStatus.Borrowed &&
            loan.ReturnedAt == null,
         ct
      );

   public async Task<IReadOnlyList<Loan>> FindBorrowedByReaderIdAsync(
      Guid readerId,
      CancellationToken ct
   ) => await loanDbContext.Loans
      .Where(loan =>
         loan.ReaderId == readerId &&
         loan.Status == LoanStatus.Borrowed &&
         loan.ReturnedAt == null
      )
      .OrderBy(loan => loan.LoanPeriodVo.DueDate)
      .ThenBy(loan => loan.LoanPeriodVo.LoanDate)
      .ThenBy(loan => loan.Id)
      .ToListAsync(
         ct
      );

   public void Add(
      Loan loan
   ) => loanDbContext.Add(
      loan: loan
   );

   public void AddRange(
      IEnumerable<Loan> loans
   ) => loanDbContext.AddRange(
      loans: loans
   );
}