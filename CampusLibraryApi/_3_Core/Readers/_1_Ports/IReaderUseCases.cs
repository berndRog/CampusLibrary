using CampusLibraryApi._2_Shared;
using CampusLibraryApi._3_Core.Readers._2_Application.Dtos;
namespace CampusLibraryApi._3_Core.Readers._1_Ports;

public interface IReaderUseCases {
   
   Task<Result<ReaderDto>> CreateAsync(
      ReaderCreateDto dto,
      CancellationToken ct
   );
   
   public interface IReaderUseCases {

      Task<Result<ReaderDto>> CreateAsync(
         ReaderCreateDto dto,
         CancellationToken ct
      );
      
   }
}