using System.ComponentModel.DataAnnotations;

namespace CampusLibraryClient.Ui.Models;

public sealed class ReaderUpdateModel {

   [Required]
   public string? Lastname { get; set; }

   [Required]
   [EmailAddress]
   public string? Email { get; set; }

   public string? Street { get; set; }

   public string? PostalCode { get; set; }

   public string? City { get; set; }

   public string? Country { get; set; }
}
