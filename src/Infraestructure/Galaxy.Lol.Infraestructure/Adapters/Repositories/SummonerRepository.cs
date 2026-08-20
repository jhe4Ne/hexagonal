using Galaxy.Lol.Domain.Entities;
using Galaxy.Lol.Domain.Model.ReadModels;
using Galaxy.Lol.Domain.Ports.Repositories;
using Galaxy.Lol.Domain.ValueObjects;
using Galaxy.Lol.Infraestructure.Configuration.Repositories.Context;
using Microsoft.EntityFrameworkCore;

namespace Galaxy.Lol.Infraestructure.Adapters.Repositories
{
    public class SummonerRepository(ChampionsDbContext context)
        : BaseRepository<Summoner>(context), ISummonerRepositoryPort
    {
        public async Task<Summoner?> GetByPuuidAsync(Puuid puuid, CancellationToken cancellationToken = default) =>
            await Entities
                .Include(s => s.Masteries)
                .FirstOrDefaultAsync(s => s.Puuid.Value == puuid.Value, cancellationToken);

        public async Task<(IReadOnlyCollection<ChampionMastery> Items, int TotalRows)> ListMasteriesAsync(
            Puuid puuid, int page, int rows, CancellationToken cancellationToken = default)
        {
            var consulta = Context.ChampionMasteries
                .AsNoTracking()
                .Where(m => Context.Summoners.Any(s => s.Id == m.SummonerId && s.Puuid.Value == puuid.Value));

            var total = await consulta.CountAsync(cancellationToken);

            var items = await consulta
                .OrderByDescending(m => m.Score.Points)
                .Skip((page - 1) * rows)
                .Take(rows)
                .ToListAsync(cancellationToken);

            return (items, total);
        }

        public async Task<IReadOnlyCollection<ChampionMastery>> GetTopMasteriesAsync(
            Puuid puuid, int count, CancellationToken cancellationToken = default) =>
            await Context.ChampionMasteries
                .AsNoTracking()
                .Where(m => Context.Summoners.Any(s => s.Id == m.SummonerId && s.Puuid.Value == puuid.Value))
                .OrderByDescending(m => m.Score.Points)
                .Take(count)
                .ToListAsync(cancellationToken);

        public async Task<IReadOnlyCollection<MasteryByRoleReadModel>> GetMasteryByRoleAsync(
            Puuid puuid, CancellationToken cancellationToken = default)
        {
            var sql = @"
                SELECT  r.role                                  AS ""Role"",
                        COUNT(DISTINCT m.champion_key)::int      AS ""Champions"",
                        COALESCE(SUM(m.points), 0)::bigint       AS ""TotalPoints"",
                        COALESCE(MAX(m.level), 0)::int           AS ""MaxLevel""
                FROM    champions.champion_mastery m
                JOIN    champions.summoner s        ON s.id = m.summoner_id
                JOIN    champions.champion_profile c ON c.champion_key = m.champion_key
                JOIN    champions.champion_role r   ON r.champion_profile_id = c.id
                WHERE   s.puuid = {0}
                GROUP BY r.role
                ORDER BY 3 DESC";

            return await Context.MasteryByRole
                .FromSqlRaw(sql, puuid.Value)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }
    }
}
