namespace CampusLibraryClient.Shared.Logging;

public static class AppDiagnosticsLogger {

   public static void LogException(
      ILogger logger,
      Exception exception,
      string title,
      string detail
   ) {
      logger.LogError(
         exception: exception,
         message: "{Title}: {Detail}",
         title,
         detail
      );
   }

   public static void LogError(
      ILogger logger,
      string title,
      string detail,
      string? extra = null
   ) {
      logger.LogError(
         "{Title}: {Detail} {Extra}",
         title,
         detail,
         extra ?? string.Empty
      );
   }

   public static void LogAuthorizationFailure(
      ILogger logger,
      string detail
   ) {
      logger.LogWarning(
         "Authorization failure: {Detail}",
         detail
      );
   }

   public static void LogTokenAttached(
      ILogger logger,
      bool hasToken,
      string? pathAndQuery
   ) {
      logger.LogInformation(
         "Outgoing API request {PathAndQuery}: bearer token present = {HasToken}",
         pathAndQuery ?? "unknown",
         hasToken
      );
   }

   public static void LogTokenRefresh(
      ILogger logger,
      TokenRefreshEvent refreshEvent,
      string detail
   ) {
      logger.LogInformation(
         "Token refresh {RefreshEvent}: {Detail}",
         refreshEvent,
         detail
      );
   }
}
