using CampusLibrary.Api.Infrastructure.Persistence;
using CampusLibrary.Api.Readers.Application.Dtos;
using CampusLibrary.Api.Readers.Application.Ports;
using CampusLibrary.Api.Readers.Domain;
using CampusLibrary.Api.Shared;
using Microsoft.EntityFrameworkCore;

namespace CampusLibrary.Api.Infrastructure.Readers;

internal sealed class ReaderReadModelEf(
   IReadersDbContext dbContext
) : IReaderReadModel {

   public async Task<Result<IReadOnlyList<ReaderDto>>> SelectAllAsync(CancellationToken ct) {
      var readers = await dbContext.Readers
         .AsNoTracking()
         .OrderBy(r => r.DisplayName)
         .Select(r => new ReaderDto(r.Id, r.Subject, r.EmailVo.Value, r.DisplayName))
         .ToListAsync(ct);

      return Result<IReadOnlyList<ReaderDto>>.Success(readers);
   }

   public async Task<Result<ReaderDto>> FindByIdAsync(Guid id, CancellationToken ct) {
      var reader = await dbContext.Readers
         .AsNoTracking()
         .Where(r => r.Id == id)
         .Select(r => new ReaderDto(r.Id, r.Subject, r.EmailVo.Value, r.DisplayName))
         .FirstOrDefaultAsync(ct);

      return reader is null
         ? Result<ReaderDto>.Failure(ReaderErrors.ReaderNotFound)
         : Result<ReaderDto>.Success(reader);
   }

   public async Task<Result<ReaderDto>> FindBySubjectAsync(string subject, CancellationToken ct) {
      var reader = await dbContext.Readers
         .AsNoTracking()
         .Where(r => r.Subject == subject)
         .Select(r => new ReaderDto(r.Id, r.Subject, r.EmailVo.Value, r.DisplayName))
         .FirstOrDefaultAsync(ct);

      return reader is null
         ? Result<ReaderDto>.Failure(ReaderErrors.ReaderNotFound)
         : Result<ReaderDto>.Success(reader);
   }
}
