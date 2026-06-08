using CampusLibraryApi._3_Core.Readers._1_Ports;
using CampusLibraryApi._3_Core.Readers._3_Domain.Entities;
using CampusLibraryApi._3_Core.Readers._3_Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace CampusLibraryApi._4_Infrastructure.Persistence.Repositories;

// EF Core implementation of the Reader repository port.
// This class belongs to Infrastructure and is intentionally internal.
// It loads and stores Reader aggregates for write-side use cases.
internal sealed class ReaderRepositoryEf(
   IReaderDbContext dbContext
) : IReaderRepository {
   // Load Reader aggregate by technical identifier.
   public async Task<Reader?> FindByIdAsync(
      Guid id,
      CancellationToken ct
   ) => await dbContext.Readers
      .FirstOrDefaultAsync(r => r.Id == id, ct);

   // Load Reader aggregate by technical identity subject.
   public async Task<Reader?> FindBySubjectAsync(
      string subject,
      CancellationToken ct
   ) => await dbContext.Readers
      .FirstOrDefaultAsync(r => r.Subject == subject, ct);

   // Load Reader aggregate by email value object.
   public async Task<Reader?> FindByEmailAsync(
      EmailVo emailVo,
      CancellationToken ct
   ) => await dbContext.Readers
      .FirstOrDefaultAsync(r => r.EmailVo == emailVo, ct);

   // Check subject uniqueness for create workflows.
   public async Task<bool> ExistsBySubjectAsync(
      string subject,
      CancellationToken ct
   ) => await dbContext.Readers
      .AnyAsync(r => r.Subject == subject, ct);

   // Add aggregate to the EF Core change tracker.
   public void Add(Reader reader) =>
      dbContext.Add(reader);

   // Add multiple aggregates to the EF Core change tracker.
   public void AddRange(IEnumerable<Reader> readers) =>
      dbContext.AddRange(readers);
}

/*
Didaktik
--------

ReaderRepositoryEf ist die technische EF-Core-Implementierung des
Repository-Ports IReaderRepository.

Die Klasse ist internal, weil Controller und Use Cases nicht wissen
sollen, welche konkrete Persistenztechnik verwendet wird. Nach außen
sichtbar ist nur das Interface im Core-Modul.

Repositories arbeiten auf der Write-Seite mit Aggregates. Sie sind
nicht für Listenansichten oder DTO-Projektionen optimiert. Dafür gibt
es ReadModels.

Lernziele
---------

- Repository-Pattern als Infrastructure-Implementierung verstehen
- internal als Schutz der technischen Implementierung nutzen
- EF Core Change Tracking auf der Write-Seite einordnen
- Repository und ReadModel in ihrer Aufgabe unterscheiden
*/
