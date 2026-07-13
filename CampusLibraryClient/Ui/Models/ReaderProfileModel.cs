using System.ComponentModel.DataAnnotations;
using CampusLibraryClient.Api.Dtos;

namespace CampusLibraryClient.Ui.Models;

public sealed class ReaderProfileModel {

   [Required]
   [StringLength(80)]
   public string? Firstname { get; set; }

   [Required]
   [StringLength(80)]
   public string? Lastname { get; set; }

   [Required]
   [StringLength(120)]
   public string? Street { get; set; }

   [Required]
   [StringLength(20)]
   public string? PostalCode { get; set; }

   [Required]
   [StringLength(80)]
   public string? City { get; set; }

   [Required]
   [StringLength(80)]
   public string? Country { get; set; } = "DE";

   public static ReaderProfileModel FromReader(
      ReaderDto reader
   ) => new() {
      Firstname = reader.Firstname,
      Lastname = reader.Lastname,
      Street = reader.AddressDto?.Street,
      PostalCode = reader.AddressDto?.PostalCode,
      City = reader.AddressDto?.City,
      Country = reader.AddressDto?.Country ?? "DE"
   };

   public ReaderProfileMeDto ToDto() => new(
      Firstname: Firstname?.Trim() ?? string.Empty,
      Lastname: Lastname?.Trim() ?? string.Empty,
      AddressDto: new AddressDto(
         Street: Street?.Trim(),
         PostalCode: PostalCode?.Trim(),
         City: City?.Trim(),
         Country: Country?.Trim()
      )
   );
}

/*
Didaktik
--------

ReaderProfileModel ist bewusst ein UI-Modell und kein API-DTO.

Die UI arbeitet mit flachen Formularfeldern. Erst beim Speichern wird daraus
für den initialen Profilabschluss das API-DTO ReaderProfileMeDto erzeugt.

Subject und fachliche Email werden hier nicht bearbeitet:
- Subject kommt aus dem IdentityAccessServer und wird beim Provisioning gesetzt.
- Reader.Email wird initial aus dem Username übernommen und kann später über
  /readers/update geändert werden.
*/
