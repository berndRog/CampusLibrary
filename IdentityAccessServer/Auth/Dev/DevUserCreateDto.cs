using System.ComponentModel.DataAnnotations;
using IdentityAccessServer.Data;
namespace IdentityAccessServer.Auth.Dev;


public sealed class DevUserCreateDto {
   
   [Required] [EmailAddress] 
   public string Email { get; init; } = string.Empty;
   public bool EmailConfirmed { get; init; } = true;
   public bool? MustChangePassword { get; init; } = true;

   [Required]
   [StringLength(100, MinimumLength = 6)]
   public string Password { get; init; } = string.Empty;
   public string? ConfirmPassword { get; init; } 

   public string? Subject { get; init; } 
   public AdminRights? AdminRights { get; init; }
   public string AccountType { get; init; } = "customer";
   
   public DateTime? CreatedAt { get; init; }

};


