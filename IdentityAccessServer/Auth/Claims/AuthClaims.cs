namespace IdentityAccessServer.Auth.Claims;

/// <summary>
/// Central definition of all claim types issued by the IdentityAccessServer.
///
/// This class is the single source of truth for:
/// - OIDC standard claims
/// - Profile-related claims
/// - Domain-specific authorization and lifecycle claims
///
/// Clients (MVC, Blazor WASM, SSR, Android, APIs) must rely on these constants.
/// </summary>
public static class AuthClaims {
   //--- Standard OIDC / JWT claims -------------------------------------------
   /// <summary>
   /// Subject identifier (OIDC mandatory).
   /// Stable technical identity of the user.
   /// </summary>
   public const string Subject = "sub";

   /// <summary>
   /// User email address.
   /// Used as primary login identifier.
   /// </summary>
   public const string Email = "email";

   /// <summary>
   /// Human-readable username for UI purposes.
   /// Typically equal to email in this system.
   /// </summary>
   public const string PreferredUsername = "preferred_username";

   /// <summary>
   /// Role claim used by ASP.NET authorization.
   /// Values: "Customer" | "Reader" | "Employee"
   /// </summary>
   public const string Role = "role";

   //--- Domain-specific claims -------------------------------------------
   /// <summary>
   /// Account classification (domain specific).
   /// Values: "customer" | "reader" | "employee"
   /// </summary>
   public const string AccountType = "account_type";

   /// <summary>
   /// Employee authorization bitmask.
   /// Serialized enum value (int).
   /// </summary>
   public const string AdminRights = "admin_rights";

   /// <summary>
   /// Indicates whether the user still has to replace an administrator-provided
   /// initial password.
   /// </summary>
   public const string MustChangePassword = "must_change_password";

   //--- Lifecycle / housekeeping claims -------
   /// <summary>
   /// Account creation timestamp (UTC, ISO-8601).
   /// Used to detect abandoned or stale accounts.
   /// </summary>
   public const string CreatedAt = "created_at";

   /// <summary>
   /// Last meaningful activity timestamp (UTC, ISO-8601).
   /// Updated on login or profile changes.
   /// </summary>
   public const string UpdatedAt = "updated_at";
}
