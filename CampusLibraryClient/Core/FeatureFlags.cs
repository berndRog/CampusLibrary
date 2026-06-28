namespace CampusLibraryClient.Core;

public static class FeatureFlags {

   // Part 5 default: the client runs without active authentication.
   public const string AuthNEnabled = "Features:AuthNEnabled";

   // Part 8 default: API calls are still anonymous in Part 5 and Part 6.
   public const string ApiAccessTokenEnabled = "Features:ApiAccessTokenEnabled";

   // Part 8 default: role/policy based UI decisions are not active yet.
   public const string AuthZEnabled = "Features:AuthZEnabled";
}
