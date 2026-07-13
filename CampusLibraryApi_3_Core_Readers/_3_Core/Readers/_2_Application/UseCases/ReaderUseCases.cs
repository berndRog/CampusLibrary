using CampusLibraryApi._2_BuildingBlocks;
using CampusLibraryApi._3_Core.Readers._1_Ports.Inbound;
using CampusLibraryApi._3_Core.Readers._2_Application.Dtos;

namespace CampusLibraryApi._3_Core.Readers._2_Application.UseCases;

// Facade for all Reader write and self-service use cases.
// Controllers depend on this facade instead of depending on every single use case.
internal sealed class ReaderUseCases(
   //ReaderUcCreate createUc,
   //ReaderUcUpdate updateUc,
   ReaderUcCreateMeProvision createMeProvisionUc,
   ReaderUcUpdateMeProfile updateMeProfileUc,
   ReaderUcUpdateMe updateMeUc,
   ReaderUcDeactivate deactivatedUc
   ) : IReaderUseCases {

   // public Task<Result<ReaderDto>> CreateAsync(
   //    ReaderCreateDto dto,
   //    CancellationToken ct
   // ) => createUc.ExecuteAsync(dto, ct);
   //
   // public Task<Result<ReaderDto>> UpdateAsync(
   //    Guid id,
   //    ReaderUpdateDto dto,
   //    CancellationToken ct
   // ) => updateUc.ExecuteAsync(id, dto, ct);
   
   public Task<Result<ReaderProvisionMeDto>> ProvisionMeAsync(
      string? id,
      CancellationToken ct
   ) => createMeProvisionUc.ExecuteAsync(id, ct);

   public Task<Result<ReaderDto>> UpdateMeProfileAsync(
      ReaderProfileMeDto meDto,
      CancellationToken ct
   ) => updateMeProfileUc.ExecuteAsync(meDto, ct);
   
   public Task<Result<ReaderDto>> UpdateMeAsync(
      ReaderUpdateMeDto meDto,
      CancellationToken ct
   ) => updateMeUc.ExecuteAsync(meDto, ct);
   
   public Task<Result> DeactivateAsync(
      Guid id,
      CancellationToken ct
   ) => deactivatedUc.ExecuteAsync(id, ct);

}

/*
Didaktik
--------

ReaderUseCases ist die konkrete Fassade für die Reader-Anwendungsfälle.

Part 6 erweitert die Fassade um Self-Service-UseCases. Die Fassade enthält
selbst keine Fachlogik. Sie delegiert nur an die konkreten UseCase-Klassen.

Dadurch bleibt im Controller eine einfache Abhängigkeit erhalten, obwohl das
Readers-Modul mehrere fachliche Abläufe besitzt.

Lernziele
---------

- Fassade als Vereinfachung für Controller-Abhängigkeiten verstehen
- klassische Verwaltung und Self-Service in einer Fassade bündeln
- Fachlogik in einzelnen UseCases halten
*/
