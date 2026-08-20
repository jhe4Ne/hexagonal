using Galaxy.Lol.Domain.Entities;
using Galaxy.Lol.Infraestructure.Configuration.Repositories.Entities;
using Microsoft.EntityFrameworkCore;

namespace Galaxy.Lol.Infraestructure.Configuration.Repositories.Context
{

    public class AnalyticsDbContext(DbContextOptions<AnalyticsDbContext> options) : DbContext(options)
    {
        public DbSet<SyncLog> SyncLogs => Set<SyncLog>();
        public DbSet<MasterySnapshot> MasterySnapshots => Set<MasterySnapshot>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new SyncLogConfiguration());
            modelBuilder.ApplyConfiguration(new MasterySnapshotConfiguration());

            base.OnModelCreating(modelBuilder);
        }
    }
}
