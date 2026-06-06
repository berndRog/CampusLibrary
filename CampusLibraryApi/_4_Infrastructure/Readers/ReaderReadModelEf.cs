using CampusLibrary.Api._2_Shared;
using CampusLibrary.Api._3_Core.Readers.Application.Dtos;
using CampusLibrary.Api._3_Core.Readers.Application.Ports;
using CampusLibrary.Api._3_Core.Readers.Domain;
using CampusLibrary.Api._4_Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
namespace CampusLibrary.Api._4_Infrastructure.Readers;

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
