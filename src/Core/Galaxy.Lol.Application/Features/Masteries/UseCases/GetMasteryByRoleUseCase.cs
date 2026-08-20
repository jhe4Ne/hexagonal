using Galaxy.Lol.Application.Features.Masteries.DTO;
using Galaxy.Lol.Application.Features.Masteries.Ports;
using Galaxy.Lol.Application.Features.Masteries.Services;
using Galaxy.Lol.Application.Results;
using Galaxy.Lol.Domain.Exceptions;
using Galaxy.Lol.Domain.Ports.Repositories;
using Galaxy.Lol.Domain.ValueObjects;

namespace Galaxy.Lol.Application.Features.Masteries.UseCases
{

    public class GetMasteryByRoleUseCase(
        SummonerMasteryLoader loader, ISummonerRepositoryPort summonerRepository) : IGetMasteryByRoleUseCase
    {
        public async Task<Result<IReadOnlyCollection<MasteryByRoleResponse>>> ExecuteAsync(
            string gameName, string tagLine, string platform, CancellationToken cancellationToken = default)
        {
            Puuid identificador;
            try
            {
                identificador = await loader.ResolveAsync(gameName, tagLine, platform, cancellationToken);
            }
            catch (RiotAccountNotFoundException ex)
            {
                return Result<IReadOnlyCollection<MasteryByRoleResponse>>.Failure(ex.Message, 404);
            }

            var resumen = await summonerRepository.GetMasteryByRoleAsync(identificador, cancellationToken);

            var respuesta = resumen
                .Select(r => new MasteryByRoleResponse(r.Role, r.Champions, r.TotalPoints, r.MaxLevel))
                .ToList();

            return Result<IReadOnlyCollection<MasteryByRoleResponse>>.Success(respuesta.AsReadOnly());
        }
    }
}
