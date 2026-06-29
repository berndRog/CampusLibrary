#nullable disable

using System.ComponentModel.DataAnnotations;
using IdentityAccessServer.Auth.Options;
using IdentityAccessServer.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace IdentityAccessServer.Areas.Identity.Pages.Account;

[Authorize]
public sealed class ChangeInitialPasswordModel : PageModel {
   private readonly UserManager<ApplicationUser> _userManager;
   private readonly SignInManager<ApplicationUser> _signInManager;
   private readonly IdentityAccessServerOptions _authServerOptions;
   private readonly ILogger<ChangeInitialPasswordModel> _logger;

   public ChangeInitialPasswordModel(
      UserManager<ApplicationUser> userManager,
      SignInManager<ApplicationUser> signInManager,
      IOptions<IdentityAccessServerOptions> authServerOptions,
      ILogger<ChangeInitialPasswordModel> logger) {
      _userManager = userManager;
      _signInManager = signInManager;
      _authServerOptions = authServerOptions.Value;
      _logger = logger;
   }

   [BindProperty]
   public InputModel Input { get; set; }

   [BindProperty(SupportsGet = true)]
   public string ReturnUrl { get; set; }

   [BindProperty(SupportsGet = true)]
   public bool RememberMe { get; set; }

   public sealed class InputModel {
      [Required]
      [DataType(DataType.Password)]
      [Display(Name = "Current password")]
      public string CurrentPassword { get; set; }

      [Required]
      [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.",
         MinimumLength = 6)]
      [DataType(DataType.Password)]
      [Display(Name = "New password")]
      public string NewPassword { get; set; }

      [DataType(DataType.Password)]
      [Display(Name = "Confirm password")]
      [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
      public string ConfirmPassword { get; set; }
   }

   public async Task<IActionResult> OnGetAsync() {
      ReturnUrl ??= Url.Content("~/");

      var user = await _userManager.GetUserAsync(User);
      if (user is null)
         return Challenge();

      if (!user.MustChangePassword)
         return RedirectToValidatedReturnUrl();

      return Page();
   }

   public async Task<IActionResult> OnPostAsync() {
      ReturnUrl ??= Url.Content("~/");

      var user = await _userManager.GetUserAsync(User);
      if (user is null)
         return Challenge();

      if (!user.MustChangePassword)
         return RedirectToValidatedReturnUrl();

      if (!ModelState.IsValid)
         return Page();

      if (Input.CurrentPassword == Input.NewPassword) {
         ModelState.AddModelError(string.Empty, "The new password must be different from the current password.");
         return Page();
      }

      var result = await _userManager.ChangePasswordAsync(user, Input.CurrentPassword, Input.NewPassword);
      if (!result.Succeeded) {
         foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);

         return Page();
      }

      user.MustChangePassword = false;
      user.UpdatedAt = DateTimeOffset.UtcNow;
      var updateResult = await _userManager.UpdateAsync(user);
      if (!updateResult.Succeeded) {
         foreach (var error in updateResult.Errors)
            ModelState.AddModelError(string.Empty, error.Description);

         return Page();
      }

      await _signInManager.RefreshSignInAsync(user);
      _logger.LogInformation("Initial password changed for user {UserId}.", user.Id);

      return RedirectToValidatedReturnUrl();
   }

   private IActionResult RedirectToValidatedReturnUrl() {
      ReturnUrl ??= Url.Content("~/");

      if (Url.IsLocalUrl(ReturnUrl))
         return LocalRedirect(ReturnUrl);

      if (TryGetAllowedAbsoluteReturnUrl(ReturnUrl, out var absoluteReturnUrl))
         return Redirect(absoluteReturnUrl);

      _logger.LogWarning("Rejected invalid return URL after initial password change: {ReturnUrl}", ReturnUrl);
      return LocalRedirect(Url.Content("~/"));
   }

   private bool TryGetAllowedAbsoluteReturnUrl(string returnUrl, out string absoluteReturnUrl) {
      absoluteReturnUrl = string.Empty;

      if (!Uri.TryCreate(returnUrl, UriKind.Absolute, out var candidate))
         return false;

      var allowedBaseUrls = new[] {
         _authServerOptions.WebBlazorSsr.BaseUrl,
         _authServerOptions.WebMvc.BaseUrl,
         _authServerOptions.BlazorWasm.BaseUrl
      };

      foreach (var baseUrl in allowedBaseUrls) {
         if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var allowed))
            continue;

         if (candidate.Scheme.Equals(allowed.Scheme, StringComparison.OrdinalIgnoreCase)
             && candidate.Host.Equals(allowed.Host, StringComparison.OrdinalIgnoreCase)
             && candidate.Port == allowed.Port) {
            absoluteReturnUrl = candidate.ToString();
            return true;
         }
      }

      return false;
   }
}
