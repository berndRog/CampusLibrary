using CampusLibraryApi._2_Shared;
using CampusLibraryApi._3_Core.Readers._1_Ports;
using CampusLibraryApi._3_Core.Readers._2_Application.Dtos;
namespace CampusLibraryApi._3_Core.Readers._2_Application.UseCases;

// facade for all read use cases
public class ReaderUseCases(
   ReaderUcCreate createUc   
): IReaderUseCases {
   
   public Task<Result<ReaderDto>> CreateAsync(
      ReaderCreateDto readerCreateDto,
      CancellationToken ct
   ) => createUc.ExecuteAsync(
      dto: readerCreateDto,
      ct: ct
   );
   
}