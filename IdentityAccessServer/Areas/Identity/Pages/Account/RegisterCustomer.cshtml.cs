#nullable disable

using System.ComponentModel.DataAnnotations;
using IdentityAccessServer.Data;
using IdentityAccessServer.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityAccessServer.Areas.Identity.Pages.Account;

[Authorize]
public sealed class RegisterCustomerModel : PageModel {
   private readonly UserManager<ApplicationUser> _userManager;
   private readonly IUserStore<ApplicationUser> _userStore;
   private readonly IUserEmailStore<ApplicationUser> _emailStore;
   private readonly IConfiguration _configuration;
   private readonly ILogger<RegisterCustomerModel> _logger;

   public RegisterCustomerModel(
      UserManager<ApplicationUser> userManager,
      IUserStore<ApplicationUser> userStore,
      IConfiguration configuration,
      ILogger<RegisterCustomerModel> logger) {
      _userManager = userManager;
      _userStore = userStore;
      _emailStore = GetEmailStore();
      _configuration = configuration;
      _logger = logger;
   }

   [BindProperty]
   public InputModel Input { get; set; }

   public string ReturnUrl { get; set; }

   [TempData]
   public string StatusMessage { get; set; }

   public string BankingCustomersUrl {
      get {
         var baseUrl = _configuration["IdentityAccessServer:BankingClientSsr:BaseUrl"]?.TrimEnd('/');
         return string.IsNullOrWhiteSpace(baseUrl)
            ? "/Identity/Account/RegisterCustomer"
            : $"{baseUrl}/customers?returnUrl=%2Femployee";
      }
   }

   public sealed class InputModel {
      [Required]
      [EmailAddress]
      [Display(Name = "Email")]
      public string Email { get; set; }

      [Required]
      [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.",
         MinimumLength = 6)]
      [DataType(DataType.Password)]
      [Display(Name = "Temporary password")]
      public string Password { get; set; }

      [DataType(DataType.Password)]
      [Display(Name = "Confirm password")]
      [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
      public string ConfirmPassword { get; set; }
   }

   public async Task<IActionResult> OnGetAsync(string returnUrl = null) {
      ReturnUrl = returnUrl;

      if (!await CanRegisterCustomersAsync())
         return Forbid();

      return Page();
   }

   public async Task<IActionResult> OnPostAsync(string returnUrl = null) {
      ReturnUrl = returnUrl;

      if (!await CanRegisterCustomersAsync())
         return Forbid();

      if (!ModelState.IsValid)
         return Page();

      var user = CreateUser();
      user.AccountType = "customer";
      user.AdminRights = AdminRights.None;
      user.EmailConfirmed = true;
      user.CreatedAt = DateTimeOffset.UtcNow;
      user.UpdatedAt = DateTimeOffset.UtcNow;

      await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
      await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

      var result = await _userManager.CreateAsync(user, Input.Password);
      if (result.Succeeded) {
         _logger.LogInformation("Customer account created by employee for {Email}.", Input.Email);
         if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return LocalRedirect(returnUrl);

         StatusMessage = $"Customer account created for {Input.Email}.";
         return RedirectToPage();
      }

      foreach (var error in result.Errors)
         ModelState.AddModelError(string.Empty, error.Description);

      return Page();
   }

   private async Task<bool> CanRegisterCustomersAsync() {
      var currentUser = await _userManager.GetUserAsync(User);
      if (currentUser is null)
         return false;

      var accountType = (currentUser.AccountType ?? "customer").Trim().ToLowerInvariant();
      return accountType == "employee"
         && (currentUser.AdminRights & AdminRights.ManageCustomers) == AdminRights.ManageCustomers;
   }

   private static ApplicationUser CreateUser() {
      try {
         return Activator.CreateInstance<ApplicationUser>();
      }
      catch {
         throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
            $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor.");
      }
   }

   private IUserEmailStore<ApplicationUser> GetEmailStore() {
      if (!_userManager.SupportsUserEmail)
         throw new NotSupportedException("The default UI requires a user store with email support.");

      return (IUserEmailStore<ApplicationUser>)_userStore;
   }
}
