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
public sealed class RegisterEmployeeModel : PageModel {
   private readonly UserManager<ApplicationUser> _userManager;
   private readonly IUserStore<ApplicationUser> _userStore;
   private readonly IUserEmailStore<ApplicationUser> _emailStore;
   private readonly IConfiguration _configuration;
   private readonly ILogger<RegisterEmployeeModel> _logger;

   public RegisterEmployeeModel(
      UserManager<ApplicationUser> userManager,
      IUserStore<ApplicationUser> userStore,
      IConfiguration configuration,
      ILogger<RegisterEmployeeModel> logger) {
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

   public string BankingEmployeesUrl {
      get {
         var baseUrl = _configuration["IdentityAccessServer:BankingClientSsr:BaseUrl"]?.TrimEnd('/');
         return string.IsNullOrWhiteSpace(baseUrl)
            ? "/Identity/Account/RegisterEmployee"
            : $"{baseUrl}/employees?returnUrl=%2Femployee";
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

      public bool ViewReports { get; set; }
      public bool ViewCustomers { get; set; }
      public bool ManageCustomers { get; set; }
      public bool ViewAccounts { get; set; }
      public bool ManageAccounts { get; set; }
      public bool ViewEmployees { get; set; } = true;
      public bool ManageEmployees { get; set; }
   }

   public async Task<IActionResult> OnGetAsync(string returnUrl = null) {
      ReturnUrl = returnUrl;

      if (!await CanRegisterEmployeesAsync())
         return Forbid();

      return Page();
   }

   public async Task<IActionResult> OnPostAsync(string returnUrl = null) {
      ReturnUrl = returnUrl;

      if (!await CanRegisterEmployeesAsync())
         return Forbid();

      if (!ModelState.IsValid)
         return Page();

      var user = CreateUser();
      user.AccountType = "employee";
      user.AdminRights = BuildAdminRights(Input);
      user.MustChangePassword = true;
      user.EmailConfirmed = true;
      user.CreatedAt = DateTime.UtcNow;
      user.UpdatedAt = DateTime.UtcNow;

      await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
      await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

      var result = await _userManager.CreateAsync(user, Input.Password);
      if (result.Succeeded) {
         _logger.LogInformation("Employee account created for {Email}.", Input.Email);
         if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return LocalRedirect(returnUrl);

         StatusMessage = $"Employee account created for {Input.Email}.";
         return RedirectToPage();
      }

      foreach (var error in result.Errors)
         ModelState.AddModelError(string.Empty, error.Description);

      return Page();
   }

   private async Task<bool> CanRegisterEmployeesAsync() {
      var currentUser = await _userManager.GetUserAsync(User);
      if (currentUser is null)
         return false;

      var accountType = (currentUser.AccountType ?? "customer").Trim().ToLowerInvariant();
      return accountType == "employee"
         && (currentUser.AdminRights & AdminRights.ManageEmployees) == AdminRights.ManageEmployees;
   }

   private static AdminRights BuildAdminRights(InputModel input) {
      var rights = AdminRights.None;

      if (input.ViewReports) rights |= AdminRights.ViewReports;
      if (input.ViewCustomers) rights |= AdminRights.ViewCustomers;
      if (input.ManageCustomers) rights |= AdminRights.ManageCustomers;
      if (input.ViewAccounts) rights |= AdminRights.ViewsAccounts;
      if (input.ManageAccounts) rights |= AdminRights.ManageAccounts;
      if (input.ViewEmployees) rights |= AdminRights.ViewEmployees;
      if (input.ManageEmployees) rights |= AdminRights.ManageEmployees;

      return rights;
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
