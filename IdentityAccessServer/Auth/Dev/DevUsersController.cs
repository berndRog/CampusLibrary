using System.ComponentModel.DataAnnotations;
using IdentityAccessServer.Data;
using IdentityAccessServer.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdentityAccessServer.Auth.Dev;

#if DEBUG
/// <summary>
/// Development-only helper endpoint to create test users via HTTP.
///
/// This intentionally bypasses the UI/form flow so API tests can create
/// disposable customer/employee accounts quickly.
///
/// NEVER use in production.
/// </summary>
[ApiController]
[ApiExplorerSettings(IgnoreApi = true)]
[Route("dev/users")]
public sealed class DevUsersController(
   IWebHostEnvironment env,
   UserManager<ApplicationUser> users,
   ILogger<DevUsersController> logger
) : ControllerBase {

   [AllowAnonymous]
   [HttpPost]
   public async Task<IActionResult> Create(
      [FromBody] DevCreateUserRequest request
   ) {
      if (!env.IsDevelopment())
         return NotFound();

      if (!ModelState.IsValid)
         return ValidationProblem(ModelState);

      var accountType = NormalizeAccountType(request.AccountType);
      if (accountType is null) {
         ModelState.AddModelError(nameof(request.AccountType), "Allowed values are 'customer' or 'employee'.");
         return ValidationProblem(ModelState);
      }

      if (!string.IsNullOrWhiteSpace(request.ConfirmPassword) &&
          !string.Equals(request.Password, request.ConfirmPassword, StringComparison.Ordinal)) {
         ModelState.AddModelError(nameof(request.ConfirmPassword), "Password and confirmation password do not match.");
         return ValidationProblem(ModelState);
      }

      var existing = await users.FindByEmailAsync(request.Email);
      if (existing is not null) {
         return Conflict(new ProblemDetails {
            Title = "User already exists",
            Detail = $"A user with email '{request.Email}' already exists.",
            Status = StatusCodes.Status409Conflict
         });
      }

      var now = DateTimeOffset.UtcNow;
      var adminRights = accountType == "employee"
         ? request.AdminRights ?? AdminRights.ViewEmployees
         : AdminRights.None;

      var mustChangePassword = accountType == "employee"
         ? request.MustChangePassword ?? true
         : false;

      var user = new ApplicationUser {
         UserName = request.Email,
         Email = request.Email,
         EmailConfirmed = request.EmailConfirmed,
         AccountType = accountType,
         AdminRights = adminRights,
         MustChangePassword = mustChangePassword,
         CreatedAt = now,
         UpdatedAt = now
      };

      var result = await users.CreateAsync(user, request.Password);
      if (!result.Succeeded) {
         foreach (var error in result.Errors)
            ModelState.AddModelError(error.Code, error.Description);

         return ValidationProblem(ModelState);
      }

      logger.LogInformation(
         "Dev user created: email='{Email}', accountType='{AccountType}', adminRights='{AdminRights}', mustChangePassword='{MustChangePassword}'",
         user.Email,
         user.AccountType,
         user.AdminRights,
         user.MustChangePassword
      );

      return Created($"/dev/users/{user.Id}", new DevCreateUserResponse(
         user.Id,
         user.Email!,
         user.AccountType,
         (int)user.AdminRights,
         user.MustChangePassword,
         user.EmailConfirmed,
         user.CreatedAt,
         user.UpdatedAt
      ));
   }

   private static string? NormalizeAccountType(string? accountType) {
      var normalized = (accountType ?? "customer").Trim().ToLowerInvariant();
      return normalized is "customer" or "employee"
         ? normalized
         : null;
   }

   public sealed class DevCreateUserRequest {
      [Required]
      [EmailAddress]
      public string Email { get; init; } = string.Empty;

      [Required]
      [StringLength(100, MinimumLength = 6)]
      public string Password { get; init; } = string.Empty;

      public string? ConfirmPassword { get; init; }

      public string AccountType { get; init; } = "customer";

      public bool EmailConfirmed { get; init; } = true;

      public bool? MustChangePassword { get; init; }

      public AdminRights? AdminRights { get; init; }
   }

   public sealed record DevCreateUserResponse(
      string Id,
      string Email,
      string AccountType,
      int AdminRights,
      bool MustChangePassword,
      bool EmailConfirmed,
      DateTimeOffset CreatedAt,
      DateTimeOffset UpdatedAt
   );
}
#endif

