namespace CampusLibrary.Shared.Logging;

/// <summary>
/// Describes the outcome of a silent token-refresh attempt.
/// </summary>
public enum TokenRefreshEvent {
   /// <summary>Token is still valid; no network call was made.</summary>
   Skipped,

   /// <summary>Token is expiring soon; a refresh request is being sent now.</summary>
   Attempting,

   /// <summary>The identity server issued a new token successfully.</summary>
   Succeeded,

   /// <summary>The refresh request failed.</summary>
   Failed
}
