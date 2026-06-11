using CampusLibraryApi._2_BuildingBlocks._1_Ports;
namespace CampusLibraryApi._4_Infrastructure;

public sealed class AppSystemClock(
   ILogger<AppSystemClock> logger
) : IClock {
   public DateTime UtcNow {
      get {
         var utcNow = DateTime.UtcNow;
         logger.LogInformation("Clock value: {Value}, Kind: {Kind}",
            utcNow, utcNow.Kind);
         return utcNow;
      }
   }
}