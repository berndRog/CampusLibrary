using CampusLibraryApi._2_BuildingBlocks;
using CampusLibraryApi._3_Core.Catalog._1_Ports.Outbound;
using CampusLibraryApi._3_Core.Catalog._2_Application.Dtos;
namespace CampusLibraryApi._3_Core.Catalog._2_Application.UseCases;

public sealed class AuthorUseCases(
   AuthorUcCreate authorUcCreate,
   AuthorUcDeactivate authorUcDeactivate
) : IAuthorUseCases {

   public async Task<Result<AuthorDto>> CreateAsync(
      AuthorCreateDto? dto,
      CancellationToken ct = default
   ) => await authorUcCreate.ExecuteAsync(
         authorCreateDto: dto,
         ct: ct
      );

   public async Task<Result<AuthorDto>> DeactivateAsync(
      Guid authorId,
      CancellationToken ct = default
   ) => await authorUcDeactivate.ExecuteAsync(
         authorId: authorId,
         ct: ct
      );
}