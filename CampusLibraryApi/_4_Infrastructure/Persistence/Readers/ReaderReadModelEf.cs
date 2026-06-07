using CampusLibraryApi._2_Shared;
using CampusLibraryApi._3_Core.Readers._1_Ports;
using CampusLibraryApi._3_Core.Readers._2_Application.Dtos;
using CampusLibraryApi._3_Core.Readers._2_Application.Mappings;
using CampusLibraryApi._3_Core.Readers._3_Domain.Errors;
using CampusLibraryApi._3_Core.Readers._3_Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
namespace CampusLibraryApi._4_Infrastructure.Persistence.ReadModels;

internal sealed class ReaderReadModelEf(
   IReaderDbContext dbContext
) : IReaderReadModel {

   public async Task<Result<ReaderDto>> FindByIdAsync(Guid id, CancellationToken ct) {
      var reader = await dbContext.Readers
         .AsNoTracking()
         .Where(r => r.Id == id)
         .Select(ReaderMappings.ToReaderDtoExpr)
         .SingleOrDefaultAsync(ct);

      return reader is null
         ? Result<ReaderDto>.Failure(ReaderErrors.ReaderNotFound)
         : Result<ReaderDto>.Success(reader);
   }

   public async Task<Result<ReaderDto>> FindBySubjectAsync(string subject, CancellationToken ct) {
      var reader = await dbContext.Readers
         .AsNoTracking()
         .Where(r => r.Subject == subject)
         .Select(ReaderMappings.ToReaderDtoExpr)
         .SingleOrDefaultAsync(ct);

      return reader is null
         ? Result<ReaderDto>.Failure(ReaderErrors.ReaderNotFound)
         : Result<ReaderDto>.Success(reader);
   }
   
   public async Task<Result<ReaderDto>> FindByEmailAsync(
      string email, 
      CancellationToken ct
   ) {
      var result = EmailVo.Create(email.Trim());
      if(result.IsFailure)
         return Result<ReaderDto>.Failure(result.Error);
      var emailVo = result.Value;
      
      var reader = await dbContext.Readers
         .AsNoTracking()
         .Where(r => r.EmailVo == emailVo)
         .Select(ReaderMappings.ToReaderDtoExpr)
         .SingleOrDefaultAsync(ct);

      return reader is null
         ? Result<ReaderDto>.Failure(ReaderErrors.ReaderNotFound)
         : Result<ReaderDto>.Success(reader);
   }
   
   public async Task<Result<IReadOnlyList<ReaderDto>>> SelectAllAsync(CancellationToken ct) {
      var readers = await dbContext.Readers
         .AsNoTracking()
         .OrderBy(r => r.Lastname)
         .ThenBy(r => r.Firstname)
         .Select(ReaderMappings.ToReaderDtoExpr)
         .ToListAsync(ct);

      return Result<IReadOnlyList<ReaderDto>>.Success(readers);
   }


}
