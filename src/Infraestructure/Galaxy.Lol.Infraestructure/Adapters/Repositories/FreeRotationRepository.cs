using Galaxy.Lol.Domain.Entities;
using Galaxy.Lol.Domain.Ports.Repositories;
using Galaxy.Lol.Infraestructure.Configuration.Repositories.Context;
using Microsoft.EntityFrameworkCore;

namespace Galaxy.Lol.Infraestructure.Adapters.Repositories
{
    public class FreeRotationRepository(ChampionsDbContext context)
        : BaseRepository<FreeRotation>(context), IFreeRotationRepositoryPort
    {
        public async Task<FreeRotation?> GetLatestAsync(string platform, CancellationToken cancellationToken = default) =>
            await Entities
                .Include(r => r.Entries)
                .Where(r => r.Platform == platform && r.IsActive)
                .OrderByDescending(r => r.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

        public async Task<IReadOnlyCollection<FreeRotation>> GetHistoryAsync(
            string platform, int take, CancellationToken cancellationToken = default) =>
            await Entities
                .AsNoTracking()
                .Include(r => r.Entries)
                .Where(r => r.Platform == platform)
                .OrderByDescending(r => r.CreatedAt)
                .Take(take)
                .ToListAsync(cancellationToken);
    }
}
