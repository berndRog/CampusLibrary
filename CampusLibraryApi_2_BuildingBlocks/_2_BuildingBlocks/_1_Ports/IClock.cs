namespace CampusLibraryApi._2_BuildingBlocks._1_Ports;

public interface IClock {
   DateTime UtcNow { get; }
}