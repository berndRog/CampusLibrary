using CampusLibraryApi._2_BuildingBlocks._2_Application.Contracts;
using CampusLibraryApi._3_Core.Readers._2_Application.Dtos;
using CampusLibraryApi._3_Core.Readers._3_Domain.Entities;
using CampusLibraryApi._3_Core.Readers._3_Domain.ValueObjects;

namespace CampusLibraryApi._3_Core.Readers._2_Application.Mappings;

public static class ReaderMappings {
   public static ReaderDto ToReaderDto(this Reader reader) =>
      new(
         Id: reader.Id,
         Firstname: reader.Firstname,
         Lastname: reader.Lastname,
         Email: reader.EmailVo.Value,
         AddressDto: reader.AddressVo.ToAddressDto(),
         IsActive: reader.IsActive,
         Subject: reader.Subject,
         IsProfileCompleted: reader.IsProfileCompleted
      );

   public static ReaderProvisionMeDto ToReaderProvisionMeDto(this Reader reader, bool wasCreated) =>
      new(
         Id: reader.Id,
         WasCreated: wasCreated
      );

   public static ReaderLoanInfoDto ToReaderLoanInfoDto(this Reader reader) =>
      new(
         Id: reader.Id,
         Firstname: reader.Firstname,
         Lastname: reader.Lastname,
         Email: reader.EmailVo.Value,
         IsActive: reader.IsActive,
         IsProfileCompleted: reader.IsProfileCompleted
      );

   public static AddressDto? ToAddressDto(this AddressVo? addressVo) =>
      addressVo is null
         ? null
         : new AddressDto(
            Street: addressVo.Street,
            PostalCode: addressVo.PostalCode,
            City: addressVo.City,
            Country: addressVo.Country
         );

   public static AddressVo ToAddressVo(this AddressDto addressDto) {
      var result = AddressVo.Create(
         street: addressDto.Street,
         postalCode: addressDto.PostalCode,
         city: addressDto.City,
         country: addressDto.Country
      );

      return result.Value;
   }
}

/*
Didaktik
--------

ReaderMappings übersetzt das Reader-Aggregate in verschiedene DTO-Sichten.

Part 6 macht diese Sichten sichtbarer:

- ReaderDto ist die eher administrative Sicht.
- ReaderMeDto ist die Self-Service-Sicht des angemeldeten Readers.
- ReaderLoanInfoDto ist der kleine Contract für das Loans-Modul.

Ein provisionierter Reader kann noch keine Adresse besitzen. Deshalb ist das
AddressDto in ReaderDto nullable. Die Profilvollständigkeit wird über
IsProfileCompleted sichtbar gemacht.

Lernziele
---------

- ein Aggregate in unterschiedliche DTO-Sichten projizieren
- Self-Service-DTOs kleiner halten als administrative DTOs
- Contract-DTOs für Modulgrenzen von internen DTOs unterscheiden
*/