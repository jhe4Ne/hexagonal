using Galaxy.Lol.Application.Features.Champions.DTO;
using Galaxy.Lol.Domain.Entities;

namespace Galaxy.Lol.Application.Features.Champions.Mappers
{

    public static class ChampionMapper
    {
        public static ChampionListItemResponse ToListItem(ChampionProfile champion, IReadOnlySet<int> rotationKeys) =>
            new(champion.ChampionId,
                champion.Key.Value,
                champion.Name,
                champion.Title,
                champion.ImageUrl,
                champion.RolPrincipal,
                champion.Roles.Select(r => r.Value).ToList(),
                champion.Difficulty.Value,
                champion.Difficulty.Category,
                champion.EsAptoParaPrincipiante,
                rotationKeys.Contains(champion.Key.Value));

        public static ChampionDetailResponse ToDetail(ChampionProfile champion, IReadOnlySet<int> rotationKeys) =>
            new(champion.ChampionId,
                champion.Key.Value,
                champion.Name,
                champion.Title,
                champion.Blurb,
                champion.ImageUrl,
                champion.Version,
                champion.Difficulty.Value,
                champion.Difficulty.Category,
                champion.EsAptoParaPrincipiante,
                rotationKeys.Contains(champion.Key.Value),
                champion.Roles.Select(r => r.Value).ToList(),
                new ChampionStatsResponse(
                    champion.Stats.Hp, champion.Stats.Mp, champion.Stats.Armor, champion.Stats.SpellBlock,
                    champion.Stats.AttackDamage, champion.Stats.AttackSpeed, champion.Stats.MoveSpeed,
                    Math.Round(champion.Stats.Tankiness, 2)),
                champion.Abilities
                    .OrderBy(a => a.Slot)
                    .Select(a => new ChampionAbilityResponse(a.Slot.ToString(), a.Name, a.Description, a.ImageUrl, a.Cooldown))
                    .ToList());
    }
}
