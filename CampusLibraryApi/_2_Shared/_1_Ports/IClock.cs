namespace CampusLibraryApi._2_Shared._1_Ports;

public interface IClock {
   DateTime UtcNow { get; }
}