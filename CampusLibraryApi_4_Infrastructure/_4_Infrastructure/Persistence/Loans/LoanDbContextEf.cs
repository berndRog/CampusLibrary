using System.Runtime.CompilerServices;
using CampusLibraryApi._3_Core.Loans._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Loans._3_Domain.Entities;
using CampusLibraryApi._4_Infrastructure.Persistence.Database;

[assembly: InternalsVisibleTo("CampusLibraryApiTest")]
namespace CampusLibraryApi._4_Infrastructure.Persistence.Readers;

internal sealed class LoadDbContextEf(
   AppDbContext db
) : ILoanDbContext {
   
   public IQueryable<Loan> Loans
      => db.Set<Loan>();

   public void Add(Loan loan)
      => db.Set<Loan>().Add(loan);

   public void AddRange(IEnumerable<Loan> loans)
      => db.Set<Loan>().AddRange(loans);
}