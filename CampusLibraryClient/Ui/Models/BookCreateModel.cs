using System.ComponentModel.DataAnnotations;

namespace CampusLibraryClient.Ui.Models;

public sealed class BookCreateModel {

   [Required]
   public string AuthorsText { get; set; } = string.Empty;

   [Required]
   public string Title { get; set; } = string.Empty;

   public string? Subtitle { get; set; }

   [Required]
   public string Isbn { get; set; } = string.Empty;
}
