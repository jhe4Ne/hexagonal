using Galaxy.Lol.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Galaxy.Lol.Infraestructure.Configuration.Repositories.Entities
{
    public class SummonerConfiguration : IEntityTypeConfiguration<Summoner>
    {
        public void Configure(EntityTypeBuilder<Summoner> builder)
        {
            builder.ToTable("summoner");

            builder.HasKey(s => s.Id);
            builder.Property(s => s.Id).HasColumnName("id");
            builder.Property(s => s.GameName).HasColumnName("game_name").HasMaxLength(60);
            builder.Property(s => s.TagLine).HasColumnName("tag_line").HasMaxLength(20);
            builder.Property(s => s.Platform).HasColumnName("platform").HasMaxLength(10).IsRequired();
            builder.Property(s => s.LastSyncAt).HasColumnName("last_sync_at");
            builder.Property(s => s.IsActive).HasColumnName("is_active");
            builder.Property(s => s.CreatedAt).HasColumnName("created_at");
            builder.Property(s => s.UpdatedAt).HasColumnName("updated_at");

            builder.OwnsOne(s => s.Puuid, puuid =>
            {
                puuid.Property(p => p.Value).HasColumnName("puuid").HasMaxLength(80).IsRequired();
                puuid.HasIndex(p => p.Value).IsUnique().HasDatabaseName("ux_summoner_puuid");
            });

            builder.HasMany(s => s.Masteries)
                   .WithOne()
                   .HasForeignKey(m => m.SummonerId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Metadata.FindNavigation(nameof(Summoner.Masteries))!
                   .SetPropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
