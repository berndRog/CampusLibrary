using CampusLibraryApi._3_Core.Readers._1_Ports;
using CampusLibraryApi._3_Core.Readers._2_Application.UseCases;
namespace CampusLibraryApi.Configure;

public static class DiReaders {

   public static IServiceCollection AddReadersModule(
      this IServiceCollection services
   ) {
      services.AddScoped<IReaderUseCases, ReaderUseCases>();
      services.AddScoped<ReaderUcCreate>();
      services.AddScoped<ReaderUcUpdate>();
      services.AddScoped<ReaderUcDeactivate>();

      return services;
   }
}
