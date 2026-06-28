using CampusLibraryClient.Core;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;

namespace CampusLibraryClient.Ui.Controllers;

// Part 5: this controller is not required for anonymous API usage.
// Part 6: it becomes useful as a post-login landing route.
[Route("entry")]
public sealed class EntryController(
   IConfiguration configuration
) : Controller {

   [HttpGet]
   public IActionResult Index() {
      if(configuration.GetValue<bool>(FeatureFlags.AuthNEnabled) &&
         User.Identity?.IsAuthenticated != true) {
         return Challenge(OpenIdConnectDefaults.AuthenticationScheme);
      }

      return Redirect("/catalog/books");
   }
}
