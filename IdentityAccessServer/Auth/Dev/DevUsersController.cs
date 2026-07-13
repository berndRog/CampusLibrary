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
      [FromBody] DevUserCreateDto dto
   ) {
      if (!env.IsDevelopment())
         return NotFound();

      if (!ModelState.IsValid)
         return ValidationProblem(ModelState);

      var accountType = NormalizeAccountType(dto.AccountType);
      if (accountType is null) {
         ModelState.AddModelError(nameof(dto.AccountType), "Allowed values are 'customer', 'reader' or 'employee'.");
         return ValidationProblem(ModelState);
      }

      if (!string.IsNullOrWhiteSpace(dto.ConfirmPassword) &&
          !string.Equals(dto.Password, dto.ConfirmPassword, StringComparison.Ordinal)) {
         ModelState.AddModelError(nameof(dto.ConfirmPassword), "Password and confirmation password do not match.");
         return ValidationProblem(ModelState);
      }

      var existing = await users.FindByEmailAsync(dto.Email);
      if (existing is not null) {
         return Conflict(new ProblemDetails {
            Title = "User already exists",
            Detail = $"A user with email '{dto.Email}' already exists.",
            Status = StatusCodes.Status409Conflict
         });
      }
      
      // min AdminRights for employees
      var adminRights = accountType == "employee"
         ? dto.AdminRights ?? AdminRights.ViewEmployees
         : AdminRights.None;

      // default: users must change their password , employees not
      // take input value when given 
      var mustChangePassword = dto.MustChangePassword ?? true;
      if (dto.MustChangePassword is null && 
          accountType == "employee") mustChangePassword = false;
      
      // sub ject must be a valid Guid 
      if (!string.IsNullOrWhiteSpace(dto.Subject?.Trim()) &&
          !Guid.TryParse(dto.Subject, out _)) {
         return BadRequest("Subject must be a valid Guid.");
      }
      var subject = string.IsNullOrWhiteSpace(dto.Subject)
         ? Guid.NewGuid().ToString()
         : dto.Subject?.Trim();
      
      // check timestamp
      var createdAt = dto.CreatedAt ?? DateTime.UtcNow;
      if( createdAt.Kind != DateTimeKind.Utc) 
         return BadRequest("CreatedAt must be UTC.");
      
      var user = new ApplicationUser {
         Id = subject!,
         UserName = dto.Email,
         Email = dto.Email,
         EmailConfirmed = dto.EmailConfirmed,
         AccountType = accountType,
         AdminRights = adminRights,
         MustChangePassword = mustChangePassword,
         CreatedAt = createdAt,
         UpdatedAt = createdAt
      };

      var result = await users.CreateAsync(user, dto.Password);
      if (!result.Succeeded) {
         foreach (var error in result.Errors)
            ModelState.AddModelError(error.Code, error.Description);

         return ValidationProblem(ModelState);
      }

      logger.LogInformation(
         "Dev user created: email='{Email}', accountType='{AccountType}', adminRights='{AdminRights}', mustChangePassword='{MustChangePassword}'",
         user.Email, user.AccountType, user.AdminRights, user.MustChangePassword);

      return Created($"/dev/users/{user.Id}", new DevUserCreateDto{
         Email = user.Email!,
         EmailConfirmed = user.EmailConfirmed,
                
         Password =  "******",
         ConfirmPassword = "******",
         MustChangePassword = user.MustChangePassword,
        
         Subject = user.Id,
         AccountType = user.AccountType,
         AdminRights = user.AdminRights,
 
         CreatedAt = user.CreatedAt
      });
   }

   private static string? NormalizeAccountType(string? accountType) {
      var normalized = (accountType ?? "customer").Trim().ToLowerInvariant();
      return normalized is "customer" or "reader" or "employee"
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

      public string? Subject { get; init; } = null;
   }
}
#endif

