using CampusLibraryApi._2_BuildingBlocks._1_Ports;
using Microsoft.Extensions.Options;

namespace CampusLibraryApi._1_Web.Security;

public sealed class DevIdentityGateway(
   IOptionsMonitor<DevIdentityOptions> optionsMonitor
) : IIdentityGateway {

   private const string ReaderAccountType = "reader";
   private const string EmployeeAccountType = "employee";

   private DevIdentityProfileOptions ActiveProfile {
      get {
         DevIdentityOptions options =
            optionsMonitor.CurrentValue;

         if(string.IsNullOrWhiteSpace(options.ActiveProfile)) {
            throw new InvalidOperationException(
               "Missing configuration value: DevIdentity:ActiveProfile"
            );
         }

         if(!options.Profiles.TryGetValue(
               options.ActiveProfile,
               out DevIdentityProfileOptions? profile
            )) {
            throw new InvalidOperationException(
               $"DevIdentity profile '{options.ActiveProfile}' was not found."
            );
         }

         return profile;
      }
   }

   private string AccountType =>
      GetRequiredValue(
         ActiveProfile.AccountType,
         nameof(DevIdentityProfileOptions.AccountType)
      );

   public string Subject =>
      GetRequiredValue(
         ActiveProfile.Subject,
         nameof(DevIdentityProfileOptions.Subject)
      );

   // Username is initially identical to the email address.
   public string Username =>
      GetRequiredValue(
         ActiveProfile.Email,
         nameof(DevIdentityProfileOptions.Email)
      );

   public DateTime CreatedAt =>
      ActiveProfile.CreatedAt;

   // Kept for compatibility with the Part 6 identity gateway.
   public int AdminRights =>
      ActiveProfile.AdminRights;

   public bool IsAuthenticated =>
      ActiveProfile.IsAuthenticated;

   public bool IsReader =>
      AccountType.Equals(
         ReaderAccountType,
         StringComparison.OrdinalIgnoreCase
      );

   public bool IsEmployee =>
      AccountType.Equals(
         EmployeeAccountType,
         StringComparison.OrdinalIgnoreCase
      );

   private static string GetRequiredValue(
      string? value,
      string propertyName
   ) {
      if(string.IsNullOrWhiteSpace(value)) {
         throw new InvalidOperationException(
            $"Missing DevIdentity profile value: {propertyName}"
         );
      }

      return value;
   }
}
