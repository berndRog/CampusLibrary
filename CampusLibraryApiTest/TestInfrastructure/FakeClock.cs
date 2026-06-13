using System.Globalization;
using CampusLibraryApi._2_BuildingBlocks._1_Ports;
namespace CampusLibraryApiTest.TestInfrastructure;

public sealed class FakeClock : IClock {
   public static readonly DateTime DefaultUtcNow = 
      DateTime.Parse(
         "2025-01-01T00:00:00Z",
         CultureInfo.InvariantCulture,
         DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal
      );

   public DateTime UtcNow { get; }

   public FakeClock() : this(DefaultUtcNow) {
   }

   public FakeClock(DateTime utcNow) {
      UtcNow = utcNow;
   }
}