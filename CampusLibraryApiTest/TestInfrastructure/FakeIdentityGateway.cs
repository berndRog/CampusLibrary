using CampusLibraryApi._2_BuildingBlocks._1_Ports;

namespace CampusLibraryApiTest.TestInfrastructure;

public sealed class FakeIdentityGateway : IIdentityGateway {
   public string Subject { get; init; } = "99000000-0000-0000-0000-000000000000";
   public string Username { get; init; } = "r.reader@library.local";
   public DateTime CreatedAt { get; init; } = FakeClock.DefaultUtcNow;
   public int AdminRights { get; init; } = 0;
   public bool IsAuthenticated { get; init; } = true;
   public bool IsReader { get; init; } = true;
   public bool IsEmployee { get; init; } = false;
}
