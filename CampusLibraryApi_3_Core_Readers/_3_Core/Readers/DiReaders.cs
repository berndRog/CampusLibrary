using CampusLibraryApi._3_Core.Readers._1_Ports;
using CampusLibraryApi._3_Core.Readers._2_Application.UseCases;
using Microsoft.Extensions.DependencyInjection;
namespace CampusLibraryApi._3_Core.Readers;

public static class DiReaders {

   public static IServiceCollection AddReadersModule(
      this IServiceCollection services
   ) {
      services.AddScoped<IReaderUseCases, ReaderUseCases>();
      services.AddScoped<ReaderUcCreate>();
      services.AddScoped<ReaderUcUpdate>();
      services.AddScoped<ReaderUcDelete>();

      return services;
   }
}
