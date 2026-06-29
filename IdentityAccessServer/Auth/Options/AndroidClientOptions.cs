namespace IdentityAccessServer.Auth.Options;

public sealed class AndroidClientOptions {
   public string ClientId { get; init; } = default!;
   public string CustomSchemeRedirectUriString { get; init; } = default!;
   public string LoopbackRedirectUriString { get; init; } = default!;
   public string PostLogoutRedirectUriString { get; init; } = default!;
   public ClientType Type { get; init; } = ClientType.Public;
}