using System.Linq.Expressions;
using CampusLibraryApi._2_BuildingBlocks;
using CampusLibraryApi._2_BuildingBlocks._1_Ports;
using CampusLibraryApi._3_Core.Loans._3_Domain.Errors;
using CampusLibraryApi._3_Core.Readers._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Readers._2_Application.Dtos;
using CampusLibraryApi._3_Core.Readers._2_Application.Mappings;
using CampusLibraryApi._3_Core.Readers._3_Domain.Entities;
using CampusLibraryApi._3_Core.Readers._3_Domain.Errors;
using CampusLibraryApi._3_Core.Readers._3_Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
namespace CampusLibraryApi._4_Infrastructure.Persistence.Readers;

// EF Core implementation of the reader read model.
// Read models are used for query operations and project database data
// directly into DTOs. They do not return domain aggregates.
internal sealed class ReaderReadModelEf(
   IIdentityGateway identityGateway,
   IReaderDbContext readerDbContext
) : IReaderReadModel {
   
   public async Task<Result<ReaderDto>> FindMeAsync(CancellationToken ct) {
      
      if (!identityGateway.IsReader)
         return Result<ReaderDto>.Failure(CommonErrors.AccessNotAllowed);
      
      // subject from Gateway
      var resultSubject = IdentitySubject.Check(identityGateway);
      if (resultSubject.IsFailure)
         return Result<ReaderDto>.Failure(resultSubject.Error);
      var subject = resultSubject.Value;

      // load Reader by subject (NO tracking, read-only)
      var readerDto = await readerDbContext.Readers
         .AsNoTracking()
         .Where(c => c.Subject == subject)    // filter by subject
         .Select(ReaderToDto)                 // project to ReaderDto (map)
         .SingleOrDefaultAsync(ct);
      
      return readerDto is null
         ? Result<ReaderDto>.Failure(CommonErrors.NotProvisioned)   
         : Result<ReaderDto>.Success(readerDto);
   }
   
   // Finds a reader by technical identifier.
   // By default, inactive readers are filtered out.
   public async Task<Result<ReaderDto>> FindByIdAsync(
      Guid id,
      bool includeInactive = false,
      CancellationToken ct = default
   ) {
      ReaderDto? dto = await readerDbContext.Readers
         .AsNoTracking()
         .Where(reader => reader.Id == id)
         .Where(reader => includeInactive || reader.IsActive)
         .Select(ReaderToDto)
         .FirstOrDefaultAsync(ct);

      if(dto is null)
         return Result<ReaderDto>.Failure(ReaderErrors.ReaderNotFound);

      return Result<ReaderDto>.Success(dto);
   }

   // Finds a reader by technical identity subject.
   // By default, inactive readers are filtered out.
   public async Task<Result<ReaderDto>> FindBySubjectAsync(
      string subject,
      bool includeInactive = false,
      CancellationToken ct = default
   ) {
      ReaderDto? dto = await readerDbContext.Readers
         .AsNoTracking()
         .Where(reader => reader.Subject == subject)
         .Where(reader => includeInactive || reader.IsActive)
         .Select(ReaderToDto)
         .FirstOrDefaultAsync(ct);

      if(dto is null)
         return Result<ReaderDto>.Failure(error: ReaderErrors.ReaderNotFound);

      return Result<ReaderDto>.Success(dto);
   }

   // Finds a reader by normalized email address.
   // By default, inactive readers are filtered out.
   public async Task<Result<ReaderDto>> FindByEmailAsync(
      string email,
      bool includeInactive = false,
      CancellationToken ct = default
   ) {
      var resultEmailVo = EmailVo.Create(email);
      if(resultEmailVo.IsFailure)
         return Result<ReaderDto>.Failure(resultEmailVo.Error);
      var emailVo = resultEmailVo.Value;
         
      ReaderDto? dto = await readerDbContext.Readers
         .AsNoTracking()
         .Where(reader => reader.EmailVo == emailVo )
         .Where(reader => includeInactive || reader.IsActive)
         .Select(ReaderToDto)
         .FirstOrDefaultAsync(ct);

      if(dto is null)
         return Result<ReaderDto>.Failure(ReaderErrors.ReaderNotFound);

      return Result<ReaderDto>.Success(dto);
   }

   // Returns readers as DTOs.
   // By default, inactive readers are filtered out.
   public async Task<Result<IReadOnlyList<ReaderDto>>> SelectAllAsync(
      bool includeInactive = false,
      CancellationToken ct = default
   ) {
      List<ReaderDto> readers = await readerDbContext.Readers
         .AsNoTracking()
         .Where(reader => includeInactive || reader.IsActive)
         .OrderBy(reader => reader.Lastname)
         .ThenBy(reader => reader.Firstname)
         .Select(ReaderToDto)
         .ToListAsync(ct);

      return Result<IReadOnlyList<ReaderDto>>.Success(readers);
   }
   
   // DTO projection used by EF Core.
   // Because this is an expression, EF Core can translate the projection
   // into SQL instead of loading full aggregates and mapping them in memory.
   private static readonly Expression<Func<Reader, ReaderDto>> ReaderToDto =
      reader => new ReaderDto(
         Id: reader.Id,
         Firstname: reader.Firstname,
         Lastname: reader.Lastname,
         Email: reader.EmailVo.Value,
         AddressDto: reader.AddressVo.ToAddressDto(),
         IsActive: reader.IsActive,
         Subject: reader.Subject,
         IsProfileCompleted: reader.Firstname != string.Empty && reader.Lastname != string.Empty
      );
   
}

/*
Didaktik
--------

Diese Klasse implementiert das ReadModel des Readers-Moduls mit EF Core.

Das ReadModel ist für lesende Abfragen zuständig. Es lädt keine vollständigen
Aggregates für die Anzeige, sondern projiziert direkt aus der Datenbank in
ReaderDto-Objekte.

Der Parameter includeInactive ersetzt die früheren zusätzlichen Methoden
FindByIdWithInactiveAsync und SelectAllWithInactiveAsync.

Standardfall:

   includeInactive = false

Dann werden nur aktive Reader geliefert.

Administrative Sicht:

   includeInactive = true

Dann werden aktive und inaktive Reader geliefert.

Die Regel ist dadurch einheitlich:

- Die normale Abfrage liefert aktive Reader.
- Die erweiterte Abfrage wird über einen Query-Parameter gesteuert.
- Es entstehen keine zusätzlichen Spezialmethoden für jede Variante.

Die Projektion ReaderToDto ist bewusst als Expression<Func<Reader, ReaderDto>>
formuliert. Dadurch kann EF Core die Projektion analysieren und in SQL
übersetzen. Eine normale Mapping-Methode wäre hier weniger geeignet, weil sie
erst nach dem Laden der Daten im Speicher ausgeführt werden könnte.

Lernziele
---------

- ReadModel als Query-Seite verstehen
- DTO-Projektion mit EF Core einsetzen
- Standardsicht und administrative Sicht über includeInactive modellieren
- Separate WithInactive-Methoden durch einen Parameter ersetzen
- Unterschied zwischen Domain-Aggregate und Anzeige-DTO erkennen
*/