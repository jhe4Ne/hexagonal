using Galaxy.Lol.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Galaxy.Lol.Infraestructure.Configuration.Repositories.Entities
{
    public class ChampionMasteryConfiguration : IEntityTypeConfiguration<ChampionMastery>
    {
        public void Configure(EntityTypeBuilder<ChampionMastery> builder)
        {
            builder.ToTable("champion_mastery");

            builder.HasKey(m => m.Id);
            builder.Property(m => m.Id).HasColumnName("id");
            builder.Property(m => m.SummonerId).HasColumnName("summoner_id");
            builder.Property(m => m.LastPlayTime).HasColumnName("last_play_time");
            builder.Property(m => m.ChestGranted).HasColumnName("chest_granted");
            builder.Property(m => m.TokensEarned).HasColumnName("tokens_earned");
            builder.Property(m => m.IsActive).HasColumnName("is_active");
            builder.Property(m => m.CreatedAt).HasColumnName("created_at");
            builder.Property(m => m.UpdatedAt).HasColumnName("updated_at");

            builder.OwnsOne(m => m.Key, key =>
            {
                key.Property(k => k.Value).HasColumnName("champion_key").IsRequired();
            });

            builder.OwnsOne(m => m.Score, score =>
            {
                score.Property(s => s.Points).HasColumnName("points");
                score.Property(s => s.Level).HasColumnName("level");
            });

            builder.HasIndex(m => m.SummonerId).HasDatabaseName("ix_champion_mastery_summoner");
        }
    }
}
