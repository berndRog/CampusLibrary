namespace IdentityAccessServer.Auth.Options;

public class ClientOptions {
   public string ClientId { get; init; } = default!;
   public string BaseUrl { get; init; } = default!;
   
   // OIDC protocol callbacks (technical)
   public string SignInCallbackPath { get; init; } = "/signin-oidc";
   public string SignOutCallbackPath { get; init; } = "/signout-callback-oidc";
   
   public string Type { get; init; } = "Confidential";
}
