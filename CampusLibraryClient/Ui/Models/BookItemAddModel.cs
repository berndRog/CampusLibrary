using System.ComponentModel.DataAnnotations;

namespace CampusLibraryClient.Ui.Models;

public sealed class BookItemAddModel {

   [Required]
   public string? InventoryNumber { get; set; }
}
