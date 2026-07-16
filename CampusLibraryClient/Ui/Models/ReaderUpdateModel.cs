using System.ComponentModel.DataAnnotations;
using CampusLibraryClient.Api.Dtos;

namespace CampusLibraryClient.Ui.Models;

public sealed class ReaderUpdateModel {

   [Required]
   [StringLength(80)]
   public string? Lastname { get; set; }

   [Required]
   [EmailAddress]
   [StringLength(120)]
   public string? Email { get; set; }

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

   public static ReaderUpdateModel FromReader(
      ReaderDto reader
   ) => new() {
      Lastname = reader.Lastname,
      Email = reader.Email,
      Street = reader.AddressDto?.Street,
      PostalCode = reader.AddressDto?.PostalCode,
      City = reader.AddressDto?.City,
      Country = reader.AddressDto?.Country ?? "DE"
   };

   public ReaderUpdateDto ToDto() => new(
      Lastname: Lastname?.Trim(),
      Email: Email?.Trim(),
      AddressDto: new AddressDto(
         Street: Street?.Trim() ?? string.Empty,
         PostalCode: PostalCode?.Trim() ?? string.Empty,
         City: City?.Trim() ?? string.Empty,
         Country: Country?.Trim()
      )
   );
}

/*
Didaktik
--------

ReaderUpdateModel beschreibt die spätere Self-Service-Änderung unter
/readers/me/update.

Änderbar sind nur:
- Nachname
- fachliche Reader-E-Mail
- Adresse

Der Vorname bleibt nach dem initialen Profilabschluss unverändert. Username,
Subject und Rolle bleiben technische IdentityAccessServer-Daten und sind hier
nicht editierbar.
*/
