using CampusLibrary.Api.Readers.Application.Dtos;
using CampusLibrary.Api.Readers.Application.Ports;
using CampusLibrary.Api.Readers.Domain;
using CampusLibrary.Api.Shared;

namespace CampusLibrary.Api.Readers.Application.UseCases;

public sealed class ReaderUcCreate(
   IReaderRepository readerRepository,
   IUnitOfWork unitOfWork
) {
   public async Task<Result<ReaderDto>> ExecuteAsync(ReaderCreateDto dto, CancellationToken ct) {
      var emailResult = EmailVo.Create(dto.Email);
      if (!emailResult.IsSuccess || emailResult.Value is null)
         return Result<ReaderDto>.Failure(emailResult.Error!);

      var subject = dto.Subject.Trim();

      if (await readerRepository.ExistsBySubjectAsync(subject, ct))
         return Result<ReaderDto>.Failure(ReaderErrors.SubjectAlreadyExists);

      var readerResult = Reader.Create(Guid.NewGuid(), subject, emailResult.Value, dto.DisplayName);
      if (!readerResult.IsSuccess || readerResult.Value is null)
         return Result<ReaderDto>.Failure(readerResult.Error!);

      var reader = readerResult.Value;
      await readerRepository.InsertAsync(reader, ct);
      await unitOfWork.SaveChangesAsync(ct);

      return Result<ReaderDto>.Success(new ReaderDto(
         reader.Id,
         reader.Subject,
         reader.EmailVo.Value,
         reader.DisplayName));
   }
}
