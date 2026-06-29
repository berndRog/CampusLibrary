using Microsoft.Extensions.Logging;

namespace CampusLibrary.Shared.Logging;

/// <summary>
/// Central diagnostics logger for the CampusLibrary learning application.
/// Produces human-readable, emoji-annotated log lines that make the
/// client/API/auth lifecycle easy to follow.
///
/// The same class is linked into the Blazor SSR client, CampusLibraryApi,
/// and IdentityAccessServer.
/// </summary>
public static class AppDiagnosticsLogger {

   internal const string SourceName = "CampusLibraryDiagnosticsLogger";

   /// <summary>
   /// Log an outgoing API call being made to another service.
   /// </summary>
   public static void LogApiCall(
      ILogger logger,
      HttpMethod method,
      string endpoint,
      string? userId = null
   ) {
      logger.LogInformation(
         "➜ API Call: {Method} {Endpoint}{User}",
         method.Method.ToUpperInvariant(),
         endpoint,
         userId is not null ? $" (User: {userId})" : string.Empty
      );
   }

   /// <summary>
   /// Log the result of an outgoing API call.
   /// </summary>
   public static void LogApiResponse(
      ILogger logger,
      HttpMethod method,
      string endpoint,
      int statusCode,
      long durationMs,
      string? errorMessage = null
   ) {
      string statusEmoji = statusCode switch {
         >= 200 and < 300 => "✓",
         >= 300 and < 400 => "→",
         >= 400 and < 500 => "⚠",
         >= 500 => "✗",
         _ => "?"
      };

      string message = errorMessage is not null
         ? $"{statusEmoji} API Response: {statusCode} after {durationMs}ms - {errorMessage}"
         : $"{statusEmoji} API Response: {statusCode} after {durationMs}ms";

      LogLevel level = statusCode switch {
         >= 200 and < 300 => LogLevel.Information,
         >= 400 and < 500 => LogLevel.Warning,
         >= 500 => LogLevel.Error,
         _ => LogLevel.Information
      };

      logger.Log(
         logLevel: level,
         message: "{Message}",
         args: message
      );
   }

   /// <summary>
   /// Log an authentication event, for example login, logout or token exchange.
   /// </summary>
   public static void LogAuthenticationEvent(
      ILogger logger,
      string eventName,
      string? details = null
   ) {
      logger.LogInformation(
         "🔐 Authentication: {Event}{Details}",
         eventName,
         details is not null ? $" - {details}" : string.Empty
      );
   }

   /// <summary>
   /// Log an authorization event, for example a user and role check.
   /// </summary>
   public static void LogAuthorizationEvent(
      ILogger logger,
      string user,
      string? roles = null
   ) {
      logger.LogInformation(
         "🔑 Authorization: User '{User}'{Roles}",
         user,
         roles is not null ? $" with roles [{roles}]" : string.Empty
      );
   }

   /// <summary>
   /// Log an authorization failure.
   /// Parameter name 'detail' is intentionally used so existing CampusLibraryClient
   /// calls with named arguments keep compiling.
   /// </summary>
   public static void LogAuthorizationFailure(
      ILogger logger,
      string detail
   ) {
      logger.LogWarning(
         "🚫 Authorization failed: {Detail}",
         detail
      );
   }

   /// <summary>
   /// Log business operation start.
   /// </summary>
   public static void LogOperationStart(
      ILogger logger,
      string operationName,
      string? context = null
   ) {
      logger.LogInformation(
         "▶ Operation: {Operation}{Context}",
         operationName,
         context is not null ? $" [{context}]" : string.Empty
      );
   }

   /// <summary>
   /// Log business operation result.
   /// </summary>
   public static void LogOperationResult(
      ILogger logger,
      string operationName,
      bool success,
      string? details = null
   ) {
      string emoji = success ? "✓" : "✗";
      LogLevel level = success ? LogLevel.Information : LogLevel.Error;

      logger.Log(
         logLevel: level,
         message: "{Emoji} Operation: {Operation} {Result}{Details}",
         args: [
            emoji,
            operationName,
            success ? "succeeded" : "failed",
            details is not null ? $" - {details}" : string.Empty
         ]
      );
   }

   /// <summary>
   /// Log an error with a student-friendly explanation.
   /// Parameter names match the current CampusLibraryClient calls.
   /// </summary>
   public static void LogError(
      ILogger logger,
      string title,
      string detail,
      string? extra = null
   ) {
      logger.LogError(
         "ERROR: {Title}\n  👤 For Student: {Detail}{Extra}",
         title,
         detail,
         extra is not null ? $"\n  🔧 Technical: {extra}" : string.Empty
      );
   }

   /// <summary>
   /// Log a successful or relevant OIDC flow step.
   /// </summary>
   public static void LogOidcFlowStep(
      ILogger logger,
      string stepName
   ) {
      logger.LogInformation(
         "🔄 OIDC Flow: {Step}",
         stepName
      );
   }

   /// <summary>
   /// Log JWT token validation result.
   /// </summary>
   public static void LogTokenValidation(
      ILogger logger,
      bool isValid,
      string? reason = null
   ) {
      if(isValid) {
         logger.LogInformation("✓ JWT Token: valid and accepted");
         return;
      }

      logger.LogWarning(
         "✗ JWT Token: invalid - {Reason}",
         reason ?? "unknown"
      );
   }

   /// <summary>
   /// Log whether a Bearer token was attached to an outgoing HTTP request.
   /// Parameter name 'pathAndQuery' is intentionally used so existing
   /// CampusLibraryClient calls with named arguments keep compiling.
   ///
   /// In Part 5, missing tokens are expected. Therefore missing tokens are logged
   /// at Debug level, not as warnings.
   /// </summary>
   public static void LogTokenAttached(
      ILogger logger,
      bool hasToken,
      string? pathAndQuery = null
   ) {
      string suffix = pathAndQuery is not null ? $" → {pathAndQuery}" : string.Empty;

      if(hasToken) {
         logger.LogDebug(
            "🎫 Bearer token attached{Endpoint}",
            suffix
         );
         return;
      }

      logger.LogDebug(
         "No Bearer token attached{Endpoint}; request is anonymous",
         suffix
      );
   }

   /// <summary>
   /// Log a step in the silent access-token refresh lifecycle.
   /// Parameter names match the current CampusLibraryClient calls.
   /// </summary>
   public static void LogTokenRefresh(
      ILogger logger,
      TokenRefreshEvent refreshEvent,
      string? detail = null
   ) {
      (string emoji, string message, LogLevel level) = refreshEvent switch {
         TokenRefreshEvent.Skipped => (
            "⏭",
            "Token still valid, refresh skipped",
            LogLevel.Debug
         ),
         TokenRefreshEvent.Attempting => (
            "🔃",
            "Token expiring; sending silent refresh",
            LogLevel.Information
         ),
         TokenRefreshEvent.Succeeded => (
            "✓",
            "Token refreshed successfully",
            LogLevel.Information
         ),
         TokenRefreshEvent.Failed => (
            "✗",
            "Silent token refresh failed",
            LogLevel.Warning
         ),
         _ => (
            "?",
            "Unknown token refresh event",
            LogLevel.Debug
         )
      };

      logger.Log(
         logLevel: level,
         message: "{Emoji} Token Refresh: {Message}{Details}",
         args: [
            emoji,
            message,
            detail is not null ? $" - {detail}" : string.Empty
         ]
      );
   }

   /// <summary>
   /// Log an incoming HTTP request on the server side.
   /// </summary>
   public static void LogIncomingRequest(
      ILogger logger,
      string method,
      string path,
      string? authenticatedUser = null
   ) {
      logger.LogInformation(
         "📥 Incoming: {Method} {Path} - user: {User}",
         method.ToUpperInvariant(),
         path,
         authenticatedUser ?? "anonymous"
      );
   }

   /// <summary>
   /// Log an exception with a student-friendly explanation and the full stack trace.
   /// Parameter names match the current CampusLibraryClient calls.
   /// </summary>
   public static void LogException(
      ILogger logger,
      Exception exception,
      string title,
      string detail
   ) {
      logger.LogError(
         exception: exception,
         message: "✗ ERROR: {Title}\n  👤 For Student: {Detail}\n  🔧 Technical: {ExceptionMessage}",
         args: [
            title,
            detail,
            exception.Message
         ]
      );
   }
}
