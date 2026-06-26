using System.Runtime.CompilerServices;
using CampusLibraryApi._2_BuildingBlocks;
using CampusLibraryApi._2_BuildingBlocks._1_Ports.Contracts;
using CampusLibraryApi._2_BuildingBlocks._2_Application.Contracts;
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

   // Finds loan-relevant information for one active reader.
   // The Loans module receives only the DTO, not the Reader aggregate.
   public async Task<Result<ReaderLoanInfoDto>> FindReaderForLoanAsync(
      Guid id,
      CancellationToken ct
   ) {
      if (id == Guid.Empty)
         return Result<ReaderLoanInfoDto>.Failure(CommonErrors.ReaderIdRequired);

      var reader = await readerDbContext.Readers
         .AsNoTracking()
         .FirstOrDefaultAsync(reader => reader.Id == id, ct);
      if (reader is null)
         return Result<ReaderLoanInfoDto>.Failure(CommonErrors.ReaderNotFound);

      if (!reader.IsActive)
         return Result<ReaderLoanInfoDto>.Failure(CommonErrors.ReaderIsDeactivated);

      ReaderLoanInfoDto dto = reader.ToReaderLoanInfoDto();
      return Result<ReaderLoanInfoDto>.Success(dto);
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

Der Contract gibt kein Reader-Aggregate zurück, sondern nur ein
ReaderLoanInfoDto. Dadurch entscheidet das Readers-Modul selbst, welche
Informationen über Reader für Ausleihvorgänge sichtbar sind.

Die Methode liefert nur aktive Reader für einen Ausleihvorgang. Deaktivierte
Reader dürfen keine neuen Ausleihen durchführen. Diese Regel wird hier an
der Modulgrenze geprüft, weil Readers weiß, ob ein Reader aktiv ist.

Damit bleibt die fachliche Zuständigkeit klar:
Readers verwaltet Reader.
Loans verwaltet Ausleihvorgänge.
*/