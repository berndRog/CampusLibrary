using CampusLibraryApi._2_Shared;
using CampusLibraryApi._2_Shared._1_Ports;
using CampusLibraryApi._2_Shared._2_Application.Utils;
using CampusLibraryApi._3_Core.Readers._1_Ports;
using CampusLibraryApi._3_Core.Readers._2_Application.Dtos;
using CampusLibraryApi._3_Core.Readers._2_Application.Mappings;
using CampusLibraryApi._3_Core.Readers._3_Domain.Entities;
using CampusLibraryApi._3_Core.Readers._3_Domain.Errors;
using CampusLibraryApi._3_Core.Readers._3_Domain.ValueObjects;
namespace CampusLibraryApi._3_Core.Readers._2_Application.UseCases;

public sealed class ReaderUcCreate(
   IReaderRepository repository,
   IUnitOfWork unitOfWork,
   IClock clock,
   ILogger<ReaderUcCreate> logger
) {
   public async Task<Result<ReaderDto>> ExecuteAsync(
      ReaderCreateDto dto,
      CancellationToken ct
   ) {
      if (dto == default)
         return Result<ReaderDto>.Failure(ReaderErrors.CustomerCreateDtoRequired);

      var subject = dto.Subject.Trim();
      if (await repository.ExistsBySubjectAsync(subject, ct))
         return Result<ReaderDto>.Failure(ReaderErrors.SubjectAlreadyExists);

      var resultEmail = EmailVo.Create(dto.Email);
      if (!resultEmail.IsSuccess || resultEmail.Value is null)
         return Result<ReaderDto>.Failure(resultEmail.Error!);
      var emailVo = resultEmail.Value;
      // check email uniqueness
      if (await repository.FindByEmailAsync(emailVo, ct) != null) {
         return Result<ReaderDto>.Failure(ReaderErrors.EmailAlreadyInUse);
      }

      // validate address if provided and create AddressVo
      var addressDto = dto.AddressDto;
      var resultAddress = AddressVo.Create(
         street: addressDto.Street,
         postalCode: addressDto.PostalCode,
         city: addressDto.City,
         country: addressDto.Country
      );
      if (resultAddress.IsFailure)
         return Result<ReaderDto>.Failure(resultAddress.Error);
      var addressVo = resultAddress.Value;

      // Resolve (or generate) aggregate id
      var resultId = EntityId.Resolve(dto.Id, ReaderErrors.InvalidEmail);
      if (resultId.IsFailure)
         return Result<ReaderDto>.Failure(resultId.Error);
      var id = resultId.Value;

      // create a Reader entity using factory method 
      var resultReader = Reader.Create(
         id: id,
         subject: subject,
         firstname: dto.Firstname,
         lastname: dto.Lastname,
         emailVo: emailVo,
         addressVo: addressVo,
         createdAt: clock.UtcNow
      );
      if (!resultReader.IsSuccess || resultReader.Value is null)
         return Result<ReaderDto>.Failure(resultReader.Error!);
      var reader = resultReader.Value;

      // Add reader to repository (tracked by EF)
      repository.Add(reader);
      
      var rows = await unitOfWork.SaveAllChangesAsync("ReadUcCreate", ct);
      
      logger.LogInformation("CustomerUcCreate={id} rows={rows}",
         reader.Id, rows);
      
      return Result<ReaderDto>.Success(reader.ToReaderDto());
   }
}