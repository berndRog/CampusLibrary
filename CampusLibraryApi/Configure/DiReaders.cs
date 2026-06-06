using CampusLibraryApi._3_Core.Readers._1_Ports;
using CampusLibraryApi._3_Core.Readers._2_Application.UseCases;
using CampusLibraryApi._4_Infrastructure.Persistence.ReadModels;
using CampusLibraryApi._4_Infrastructure.Persistence.Repositories;
namespace CampusLibraryApi.Configure;

public static class DiReaders {

   public static IServiceCollection AddReadersModule(
      this IServiceCollection services
   ) {
      services.AddScoped<IReaderRepository, ReaderRepositoryEf>();
      services.AddScoped<IReaderReadModel, ReaderReadModelEf>();
      services.AddScoped<ReaderUcCreate>();

      return services;
   }
}
