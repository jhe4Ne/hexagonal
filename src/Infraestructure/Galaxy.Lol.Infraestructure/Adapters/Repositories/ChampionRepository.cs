using Galaxy.Lol.Domain.Entities;
using Galaxy.Lol.Domain.Model.ReadModels;
using Galaxy.Lol.Domain.Ports.Repositories;
using Galaxy.Lol.Domain.ValueObjects;
using Galaxy.Lol.Infraestructure.Configuration.Repositories.Context;
using Microsoft.EntityFrameworkCore;

namespace Galaxy.Lol.Infraestructure.Adapters.Repositories
{
    public class ChampionRepository(ChampionsDbContext context)
        : BaseRepository<ChampionProfile>(context), IChampionRepositoryPort
    {
        public async Task<ChampionProfile?> GetByChampionIdAsync(string championId, CancellationToken cancellationToken = default) =>
            await Entities.FirstOrDefaultAsync(c => c.ChampionId == championId, cancellationToken);

        public async Task<ChampionProfile?> GetByKeyAsync(ChampionKey key, CancellationToken cancellationToken = default) =>
            await Entities.FirstOrDefaultAsync(c => c.Key.Value == key.Value, cancellationToken);

        public async Task<ChampionProfile?> GetDetailAsync(string championId, CancellationToken cancellationToken = default) =>
            await Entities
                .Include(c => c.Abilities)
                .FirstOrDefaultAsync(c => c.ChampionId == championId, cancellationToken);

        public async Task<IReadOnlyCollection<ChampionProfile>> GetAllAsync(CancellationToken cancellationToken = default) =>
            await Entities.OrderBy(c => c.Name).ToListAsync(cancellationToken);

        public async Task<IReadOnlyCollection<ChampionProfile>> GetByKeysAsync(
            IReadOnlyCollection<int> keys, CancellationToken cancellationToken = default)
        {
            if (keys.Count == 0) return [];

            return await Entities
                .AsNoTracking()
                .Where(c => keys.Contains(c.Key.Value))
                .ToListAsync(cancellationToken);
        }

        public async Task<(IReadOnlyCollection<ChampionProfile> Items, int TotalRows)> SearchAsync(
            string? filter, string? role, int? minDifficulty, int? maxDifficulty,
            IReadOnlyCollection<int>? onlyKeys, int page, int rows,
            CancellationToken cancellationToken = default)
        {
            var consulta = Entities.AsNoTracking().Where(c => c.IsActive);

            if (!string.IsNullOrWhiteSpace(filter))
            {
                var patron = $"%{filter.Trim()}%";
                consulta = consulta.Where(c => EF.Functions.ILike(c.Name, patron)
                                            || EF.Functions.ILike(c.Title, patron)
                                            || EF.Functions.ILike(c.ChampionId, patron));
            }

            if (!string.IsNullOrWhiteSpace(role))
                consulta = consulta.Where(c => c.Roles.Any(r => r.Value == role));

            if (minDifficulty.HasValue)
                consulta = consulta.Where(c => c.Difficulty.Value >= minDifficulty.Value);

            if (maxDifficulty.HasValue)
                consulta = consulta.Where(c => c.Difficulty.Value <= maxDifficulty.Value);

            if (onlyKeys is { Count: > 0 })
                consulta = consulta.Where(c => onlyKeys.Contains(c.Key.Value));

            var total = await consulta.CountAsync(cancellationToken);

            var items = await consulta
                .OrderBy(c => c.Name)
                .Skip((page - 1) * rows)
                .Take(rows)
                .ToListAsync(cancellationToken);

            return (items, total);
        }

        public async Task<IReadOnlyCollection<RoleDistributionReadModel>> GetRoleDistributionAsync(
            IReadOnlyCollection<int> rotationKeys, CancellationToken cancellationToken = default)
        {
            var claves = rotationKeys.Count == 0 ? new[] { -1 } : rotationKeys.ToArray();

            var sql = @"
                SELECT  r.role                                        AS ""Role"",
                        COUNT(*)::int                                 AS ""Total"",
                        AVG(c.difficulty)::float8                     AS ""AverageDifficulty"",
                        COUNT(*) FILTER (WHERE c.champion_key = ANY({0}))::int AS ""InFreeRotation""
                FROM    champions.champion_role r
                JOIN    champions.champion_profile c ON c.id = r.champion_profile_id
                WHERE   c.is_active = TRUE
                GROUP BY r.role
                ORDER BY 2 DESC";

            return await Context.RoleDistribution
                .FromSqlRaw(sql, claves)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }
    }
}
