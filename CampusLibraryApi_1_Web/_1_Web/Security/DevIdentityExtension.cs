using CampusLibraryApi._2_BuildingBlocks._1_Ports;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CampusLibraryApi._1_Web.Security;

public static class DevIdentityServiceExtensions {

   public static IServiceCollection AddDevIdentityGateway(
      this IServiceCollection services,
      IConfiguration configuration
   ) {
      services
         .AddOptions<DevIdentityOptions>()
         .Bind(configuration.GetSection(DevIdentityOptions.SectionName))
         .Validate(
            options => !string.IsNullOrWhiteSpace(options.ActiveProfile),
            "DevIdentity:ActiveProfile is required."
         )
         .Validate(
            options => options.Profiles.ContainsKey(options.ActiveProfile),
            "The active DevIdentity profile does not exist."
         )
         .Validate(
            options => options.Profiles.Values.All(profile =>
               !string.IsNullOrWhiteSpace(profile.Subject) &&
               !string.IsNullOrWhiteSpace(profile.AccountType) &&
               !string.IsNullOrWhiteSpace(profile.Email) &&
               profile.CreatedAt != default
            ),
            "Every DevIdentity profile requires Subject, AccountType, Email and CreatedAt."
         )
         .ValidateOnStart();

      services.AddScoped<IIdentityGateway, DevIdentityGateway>();

      return services;
   }
}
