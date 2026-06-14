using System.Runtime.CompilerServices;
using CampusLibraryApi._3_Core.Catalog._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Catalog._2_Application.UseCases;
using Microsoft.Extensions.DependencyInjection;
namespace CampusLibraryApi._3_Core;

public static class DiCatalogModule {

   public static IServiceCollection AddCatalogModule(
      this IServiceCollection services
   ) {

      services.AddScoped<AuthorUcCreate>();
      services.AddScoped<AuthorUcDeactivate>();
      services.AddScoped<IAuthorUseCases, AuthorUseCases>();

      services.AddScoped<BookUcCreate>();
      services.AddScoped<BookUcAddBookItem>();
      services.AddScoped<BookUcAssignAuthor>();
      services.AddScoped<BookUcDeactivate>();
      services.AddScoped<IBookUseCases, BookUseCases>();
      
      return services;
   }
}