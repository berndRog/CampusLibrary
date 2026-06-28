using System.ComponentModel.DataAnnotations;

namespace CampusLibraryClient.Ui.Models;

public sealed class BookCreateModel {

   [Required]
   public string? AuthorsText { get; set; }

   [Required]
   public string? Title { get; set; }

   public string? Subtitle { get; set; }

   [Required]
   public string? Isbn { get; set; }
}
