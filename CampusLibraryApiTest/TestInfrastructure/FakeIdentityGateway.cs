using CampusLibraryApi._2_BuildingBlocks._1_Ports;

namespace CampusLibraryApiTest.TestInfrastructure;

public sealed class FakeIdentityGateway : IIdentityGateway {
   public string Subject { get; init; } = "part6-reader-subject-001";
   public string Username { get; init; } = "reader.part6@example.org";
   public DateTime CreatedAt { get; init; } = FakeClock.DefaultUtcNow;
   public int AdminRights { get; init; } = 0;
   public bool IsAuthenticated { get; init; } = true;
   public bool IsReader { get; init; } = true;
   public bool IsEmployee { get; init; } = false;
}
