using System.ComponentModel.DataAnnotations;

namespace CampusLibraryClient.Ui.Models;

public sealed class LoanCreateModel {

   [Required]
   public string? ReaderId { get; set; }

   [Required]
   public string? BookItemId { get; set; }
}
