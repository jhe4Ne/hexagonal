using Galaxy.Lol.Application.Features.Rotations.DTO;
using Galaxy.Lol.Application.Features.Rotations.Ports;
using Galaxy.Lol.Application.Results;
using Galaxy.Lol.Domain.Entities;
using Galaxy.Lol.Domain.Ports.Repositories;

namespace Galaxy.Lol.Application.Features.Rotations.UseCases
{

    public class GetFreeRotationUseCase(
        IFreeRotationRepositoryPort rotationRepository,
        IChampionRepositoryPort championRepository) : IGetFreeRotationUseCase
    {
        public async Task<Result<FreeRotationResponse>> ExecuteAsync(
            string platform, CancellationToken cancellationToken = default)
        {
            var rotacion = await rotationRepository.GetLatestAsync(platform, cancellationToken);
            if (rotacion is null)
                return Result<FreeRotationResponse>.Failure(
                    $"Todavia no se ha sincronizado la rotacion de la plataforma '{platform}'.", 404);

            var respuesta = await ConstruirRespuestaAsync(rotacion, championRepository, cancellationToken);
            return Result<FreeRotationResponse>.Success(respuesta);
        }

        internal static async Task<FreeRotationResponse> ConstruirRespuestaAsync(
            FreeRotation rotacion, IChampionRepositoryPort championRepository, CancellationToken cancellationToken)
        {
            var claves = rotacion.Entries.Select(e => e.Key.Value).Distinct().ToList();
            var campeones = await championRepository.GetByKeysAsync(claves, cancellationToken);
            var porClave = campeones.ToDictionary(c => c.Key.Value);

            var detalle = rotacion.Entries
                .Select(e =>
                {
                    porClave.TryGetValue(e.Key.Value, out var champion);
                    return new RotationChampionResponse(
                        e.Key.Value,
                        champion?.ChampionId ?? string.Empty,

                        champion?.Name ?? $"Campeon {e.Key.Value}",
                        champion?.Title ?? string.Empty,
                        champion?.ImageUrl,
                        champion?.RolPrincipal ?? "Sin clasificar",
                        champion?.Difficulty.Value ?? 0,
                        champion?.Difficulty.Category ?? "Desconocida",
                        e.ForNewPlayers);
                })
                .OrderBy(c => c.ForNewPlayers)
                .ThenBy(c => c.Name)
                .ToList();

            return new FreeRotationResponse(
                rotacion.Platform,
                rotacion.Period.Start,
                rotacion.Period.End,
                rotacion.EstaVigente(DateTime.UtcNow),
                rotacion.MaxNewPlayerLevel,
                rotacion.ClavesGenerales.Count,
                rotacion.Hash,
                rotacion.CreatedAt,
                detalle);
        }
    }
}
