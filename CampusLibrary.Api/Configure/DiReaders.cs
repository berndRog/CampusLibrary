using CampusLibrary.Api.Infrastructure.Readers;
using CampusLibrary.Api.Readers.Application.Ports;
using CampusLibrary.Api.Readers.Application.UseCases;

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
