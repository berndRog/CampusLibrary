namespace CampusLibraryClient.Core;

public static class FeatureFlags {

   // Part 6: activates real OpenID Connect login/logout in the SSR client.
   public const string AuthNEnabled = "Features:AuthNEnabled";

   // Part 5: simulates an authenticated reader or employee without a real login.
   // This is only a UI teaching aid and must not be treated as security.
   public const string DevIdentityEnabled = "Features:DevIdentityEnabled";

   // Part 6: forwards the access token for the protected Reader /me flow.
   // Part 7 hardens all API client calls systematically.
   public const string ApiAccessTokenEnabled = "Features:ApiAccessTokenEnabled";

   // Part 6: activates role/policy based protection for routable Blazor pages.
   // Part 7 hardens API endpoints, API clients and deeper use-case checks.
   public const string AuthZEnabled = "Features:AuthZEnabled";
}
