namespace CampusLibraryClient.Core;

public static class FeatureFlags {

   // Part 6: activates real OpenID Connect login/logout in the SSR client.
   public const string AuthNEnabled = "Features:AuthNEnabled";

   // Part 5: simulates an authenticated reader or employee without a real login.
   // This is only a UI teaching aid and must not be treated as security.
   public const string DevIdentityEnabled = "Features:DevIdentityEnabled";

   // Part 8 default: API calls are still anonymous in Part 5 and Part 6.
   public const string ApiAccessTokenEnabled = "Features:ApiAccessTokenEnabled";

   // Part 8 default: role/policy based UI decisions are not active yet.
   public const string AuthZEnabled = "Features:AuthZEnabled";
}
