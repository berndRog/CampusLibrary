using System.Runtime.CompilerServices;
using CampusLibraryApi._2_BuildingBlocks;
using CampusLibraryApi._3_Core.Readers._1_Ports;
using CampusLibraryApi._3_Core.Readers._2_Application.Dtos;
using CampusLibraryApi._3_Core.Readers._2_Application.Mappings;
using CampusLibraryApi._3_Core.Readers._3_Domain.Errors;
using CampusLibraryApi._3_Core.Readers._3_Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
[assembly: InternalsVisibleTo("CampusLibraryApiTest")]
namespace CampusLibraryApi._4_Infrastructure.Persistence.ReadModels;

// EF Core read model for querying reader data.
// Projects database rows directly into DTOs and does not expose aggregates.
// This class belongs to the query side and is intentionally internal.
internal sealed class ReaderReadModelEf(
   IReaderDbContext dbContext
) : IReaderReadModel {
   
   // Find active reader DTO by technical identifier.
   public async Task<Result<ReaderDto>> FindByIdAsync(
      Guid id, 
      CancellationToken ct
   ) {
      var reader = await dbContext.Readers
         .AsNoTracking()
         .SingleOrDefaultAsync(r => r.Id == id && r.IsActive, ct);

      return reader is null
         ? Result<ReaderDto>.Failure(ReaderErrors.ReaderNotFound)
         : Result<ReaderDto>.Success(reader.ToReaderDto());
   }

   // Find active reader DTO by technical identity subject.
   public async Task<Result<ReaderDto>> FindBySubjectAsync(
      string subject, 
      CancellationToken ct
   ) {
      var reader = await dbContext.Readers
         .AsNoTracking()
         .SingleOrDefaultAsync(r => 
            r.Subject == subject && r.IsActive,ct);

      return reader is null
         ? Result<ReaderDto>.Failure(ReaderErrors.ReaderNotFound)
         : Result<ReaderDto>.Success(reader.ToReaderDto());
   }

   // Find a reader DTO by email address.
   // The email is normalized through EmailVo before the database query.
   public async Task<Result<ReaderDto>> FindByEmailAsync(
      string email,
      CancellationToken ct
   ) {
      var result = EmailVo.Create(email.Trim());
      if (result.IsFailure)
         return Result<ReaderDto>.Failure(result.Error);

      var emailVo = result.Value;

      var reader = await dbContext.Readers
         .AsNoTracking()
         .SingleOrDefaultAsync(r => 
            r.EmailVo == emailVo && r.IsActive, ct);

      return reader is null
         ? Result<ReaderDto>.Failure(ReaderErrors.ReaderNotFound)
         : Result<ReaderDto>.Success(reader.ToReaderDto());
   }

   // Return all active readers ordered for display.
   public async Task<Result<IReadOnlyList<ReaderDto>>> SelectAllAsync(CancellationToken ct) {
      var readers = await dbContext.Readers
         .AsNoTracking()
         .Where(r => r.IsActive)
         .OrderBy(r => r.Lastname)
         .ThenBy(r => r.Firstname)
         .ToListAsync(ct);
      
      var readerDtos = readers
         .Select(r => r.ToReaderDto())
         .ToList();

      return Result<IReadOnlyList<ReaderDto>>.Success(readerDtos);
   }

   // Find active reader DTO by technical identifier.
   public async Task<Result<ReaderDto>> FindByIdWithInactiveAsync(
      Guid id, 
      CancellationToken ct = default
   ) {
      var reader = await dbContext.Readers
         .AsNoTracking()
         .SingleOrDefaultAsync(r => r.Id == id, ct);

      return reader is null
         ? Result<ReaderDto>.Failure(ReaderErrors.ReaderNotFound)
         : Result<ReaderDto>.Success(reader.ToReaderDto());
   }
   
   // Return all readers ordered for display.
   public async Task<Result<IReadOnlyList<ReaderDto>>> SelectAllWithInactiveAsync(CancellationToken ct) {
      var readers = await dbContext.Readers
         .AsNoTracking()
         .OrderBy(r => r.Lastname)
         .ThenBy(r => r.Firstname)
         .ToListAsync(ct);
      
      var readerDtos = readers
         .Select(r => r.ToReaderDto())
         .ToList();

      return Result<IReadOnlyList<ReaderDto>>.Success(readerDtos);
   }
}

/*
Didaktik
--------

ReaderReadModelEf ist die technische EF-Core-Implementierung des
ReadModel-Ports IReaderReadModel.

ReadModels gehören zur Query-Seite. Sie laden keine Aggregates für
fachliche Änderungen, sondern projizieren Daten direkt in DTOs. Deshalb
wird AsNoTracking() verwendet: EF Core muss diese Objekte nicht für
spätere Änderungen überwachen.

Die Projektion erfolgt über ReaderMappings.ToReaderDtoExpr. Da dies eine
Expression ist, kann EF Core die Projektion in die Datenbankabfrage
übersetzen.

Lernziele
---------

- ReadModel als Query-Seite der Anwendung verstehen
- AsNoTracking() für reine Lesezugriffe einsetzen
- DTO-Projektion statt Aggregate-Laden nutzen
- Expression-basierte Mappings für EF-Core-Abfragen einordnen
- NotFound als erwartbaren Fehler über Result modellieren
*/
