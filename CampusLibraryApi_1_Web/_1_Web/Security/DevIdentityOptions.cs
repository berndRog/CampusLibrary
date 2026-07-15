namespace CampusLibraryApi._1_Web.Security;

public sealed class DevIdentityOptions {

   public const string SectionName = "DevIdentity";

   public string ActiveProfile { get; set; } = string.Empty;

   public Dictionary<string, DevIdentityProfileOptions> Profiles {
      get;
      set;
   } = new();
}

public sealed class DevIdentityProfileOptions {

   public bool IsAuthenticated { get; set; }

   public string Subject { get; set; } = string.Empty;

   public string AccountType { get; set; } = string.Empty;

   // Initial technical username and email are identical.
   public string Email { get; set; } = string.Empty;

   public DateTime CreatedAt { get; set; }

   public int AdminRights { get; set; }
}
