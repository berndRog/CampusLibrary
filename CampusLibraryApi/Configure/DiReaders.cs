using CampusLibrary.Api._3_Core.Readers.Application.Ports;
using CampusLibrary.Api._3_Core.Readers.Application.UseCases;
using CampusLibrary.Api._4_Infrastructure.Readers;
namespace CampusLibrary.Api.Configure;

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
