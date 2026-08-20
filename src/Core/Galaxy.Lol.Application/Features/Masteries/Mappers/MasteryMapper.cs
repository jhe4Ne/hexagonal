using Galaxy.Lol.Application.Features.Masteries.DTO;
using Galaxy.Lol.Domain.Entities;
using Galaxy.Lol.Domain.Services;

namespace Galaxy.Lol.Application.Features.Masteries.Mappers
{
    public static class MasteryMapper
    {

        public static MasteryItemResponse ToItem(
            ChampionMastery mastery,
            IReadOnlyDictionary<int, ChampionProfile> catalogo,
            IReadOnlySet<int> rotationKeys,
            IDominanceIndexCalculator calculator,
            long puntosMaximos,
            DateTime ahora)
        {
            catalogo.TryGetValue(mastery.Key.Value, out var champion);

            return new MasteryItemResponse(
                mastery.Key.Value,
                champion?.ChampionId ?? string.Empty,
                champion?.Name ?? $"Campeon {mastery.Key.Value}",
                champion?.ImageUrl,
                champion?.RolPrincipal ?? "Sin clasificar",
                mastery.Score.Level,
                mastery.Score.Points,
                calculator.Calcular(mastery.Score, puntosMaximos),
                mastery.LastPlayTime,
                mastery.ChestGranted,
                mastery.TokensEarned,
                rotationKeys.Contains(mastery.Key.Value),
                mastery.EstaAbandonado(ahora));
        }
    }
}
