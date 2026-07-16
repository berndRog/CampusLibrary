using CampusLibraryApi._3_Core.Loans._1_Ports.Inbound;
using CampusLibraryApi._3_Core.Loans._2_Application.UseCases;
using Microsoft.Extensions.DependencyInjection;
namespace CampusLibraryApi._3_Core.Loans;

public static class DiLoansModule {

   public static IServiceCollection AddLoansModule(
      this IServiceCollection services
   ) {
      services.AddScoped<ILoanUseCases, LoanUseCases>();
      services.AddScoped<LoanUcBorrow>();
      services.AddScoped<LoanUcRenew>();
      services.AddScoped<LoanUcReturnAtDesk>();

      return services;
   }
}
