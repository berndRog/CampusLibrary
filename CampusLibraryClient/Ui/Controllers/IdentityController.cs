using CampusLibraryClient.Core;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;

namespace CampusLibraryClient.Ui.Controllers;

[Route("identity")]
public sealed class IdentityController(
   IConfiguration configuration
) : Controller {

   [HttpGet("login")]
   public IActionResult Login(
      [FromQuery] string returnUrl = "/entry"
   ) {
      if(!configuration.GetValue<bool>(FeatureFlags.AuthNEnabled))
         return NotFound("Authentication is prepared, but disabled in Part 5.");

      AuthenticationProperties properties = new() {
         RedirectUri = string.IsNullOrWhiteSpace(returnUrl) ? "/entry" : returnUrl
      };

      return Challenge(
         properties: properties,
         authenticationSchemes: OpenIdConnectDefaults.AuthenticationScheme
      );
   }

   [HttpGet("logout")]
   public IActionResult Logout() {
      if(!configuration.GetValue<bool>(FeatureFlags.AuthNEnabled))
         return Redirect("/");

      AuthenticationProperties properties = new() {
         RedirectUri = "/"
      };

      return SignOut(
         properties: properties,
         authenticationSchemes: [
            CookieAuthenticationDefaults.AuthenticationScheme,
            OpenIdConnectDefaults.AuthenticationScheme
         ]
      );
   }
}
