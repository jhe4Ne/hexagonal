using Galaxy.Lol.Domain.Entities;
using Galaxy.Lol.Domain.Model.ReadModels;
using Microsoft.EntityFrameworkCore;

namespace Galaxy.Lol.Infraestructure.Configuration.Repositories.Context
{

    public class ChampionsDbContext(DbContextOptions<ChampionsDbContext> options) : DbContext(options)
    {
        public DbSet<ChampionProfile> Champions => Set<ChampionProfile>();
        public DbSet<ChampionAbility> ChampionAbilities => Set<ChampionAbility>();
        public DbSet<FreeRotation> FreeRotations => Set<FreeRotation>();
        public DbSet<FreeRotationEntry> FreeRotationEntries => Set<FreeRotationEntry>();
        public DbSet<Summoner> Summoners => Set<Summoner>();
        public DbSet<ChampionMastery> ChampionMasteries => Set<ChampionMastery>();

        public DbSet<RoleDistributionReadModel> RoleDistribution => Set<RoleDistributionReadModel>();
        public DbSet<MasteryByRoleReadModel> MasteryByRole => Set<MasteryByRoleReadModel>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("champions");

            modelBuilder.Entity<RoleDistributionReadModel>().HasNoKey().ToView(null);
            modelBuilder.Entity<MasteryByRoleReadModel>().HasNoKey().ToView(null);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ChampionsDbContext).Assembly,
                t => t.Namespace!.Contains("Repositories.Entities") &&
                     !t.Name.StartsWith("SyncLog") && !t.Name.StartsWith("MasterySnapshot"));

            base.OnModelCreating(modelBuilder);
        }
    }
}
