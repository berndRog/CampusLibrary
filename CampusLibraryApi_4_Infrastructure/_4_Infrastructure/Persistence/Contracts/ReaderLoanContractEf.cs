using System.Runtime.CompilerServices;
using CampusLibraryApi._2_BuildingBlocks;
using CampusLibraryApi._2_BuildingBlocks._1_Ports.Contracts;
using CampusLibraryApi._2_BuildingBlocks._2_Application.Dtos;
using CampusLibraryApi._3_Core.Loans._3_Domain.Errors;
using CampusLibraryApi._3_Core.Readers._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Readers._2_Application.Mappings;
using Microsoft.EntityFrameworkCore;

[assembly: InternalsVisibleTo("CampusLibraryApiTest")]
namespace CampusLibraryApi._4_Infrastructure.Persistence.Contracts;

// EF Core implementation of the Readers contract used by the Loans module.
// This class is allowed to access the Readers table because Readers owns it.
internal sealed class ReaderLoanContractEf(
   IReaderDbContext readerDbContext
) : IReaderLoanContract {

   // Finds loan-relevant information for creating a new loan.
   // The Reader must exist, be active and have a completed profile.
   public async Task<Result<ReaderLoanInfoDto>> FindReaderForLoanAsync(
      Guid readerId,
      CancellationToken ct
   ) {
      var result = await FindReaderAsync(readerId: readerId, ct: ct);

      if(result.IsFailure)
         return result;

      ReaderLoanInfoDto readerLoanInfoDto = result.Value;
      if(!readerLoanInfoDto.IsActive)
         return Result<ReaderLoanInfoDto>.Failure(CommonErrors.ReaderIsDeactivated);
      
      return result;
   }

   // Finds reader data for an already existing loan.
   // Existing loans must remain readable even if the Reader was deactivated.
   public Task<Result<ReaderLoanInfoDto>> FindReaderForExistingLoanAsync(
      Guid readerId,
      CancellationToken ct
   ) => FindReaderAsync(
      readerId: readerId,
      ct: ct
   );

   private async Task<Result<ReaderLoanInfoDto>> FindReaderAsync(
      Guid readerId,
      CancellationToken ct
   ) {
      if(readerId == Guid.Empty)
         return Result<ReaderLoanInfoDto>.Failure(
            CommonErrors.ReaderIdRequired
         );

      var reader = await readerDbContext.Readers
         .AsNoTracking()
         .FirstOrDefaultAsync(
            reader => reader.Id == readerId,
            ct
         );

      if(reader is null)
         return Result<ReaderLoanInfoDto>.Failure(
            CommonErrors.ReaderNotFound
         );

      return Result<ReaderLoanInfoDto>.Success(
         reader.ToReaderLoanInfoDto()
      );
   }
}

/*
Lernziele und Didaktik
----------------------

Diese Klasse implementiert einen Contract des Readers-Moduls für das
Loans-Modul.

Die Implementierung liegt technisch in Infrastructure, weil hier EF Core
für den Datenbankzugriff verwendet wird. Fachlich gehört der Contract aber
zum Readers-Modul, weil Readers die Reader-Daten besitzt.

Das Loans-Modul darf nicht direkt auf die Readers-Tabelle oder die
Reader-Entity zugreifen. Es fragt stattdessen diesen Contract.

Der Contract unterscheidet zwei fachlich verschiedene Anwendungsfälle:

1. Neue Ausleihe
   FindReaderForLoanAsync verlangt einen aktiven Reader mit vollständigem
   Profil. Ein deaktivierter Reader darf nichts Neues ausleihen.

2. Bestehende Ausleihe lesen
   FindReaderForExistingLoanAsync liefert auch einen inzwischen deaktivierten
   Reader. Eine vorhandene Ausleihe darf durch eine spätere Deaktivierung
   nicht unsichtbar werden.

Diese Trennung verhindert, dass eine Regel für neue Commands versehentlich
historische oder aktuell bestehende Daten aus ReadModels entfernt.
*/
