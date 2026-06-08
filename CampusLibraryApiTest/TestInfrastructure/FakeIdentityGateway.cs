namespace CampusLibraryApiTest.TestInfrastructure;

public sealed class FakeIdentityGateway{ // : IIdentityGateway {
   public string Subject { get; init; } = string.Empty;
   public string Username { get; init; } = string.Empty;
   public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
   public int AdminRights { get; init; } = 0;
   public bool IsCustomer { get; } = false;
   public bool IsEmployee { get; } = false;
}