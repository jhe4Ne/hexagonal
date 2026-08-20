using Galaxy.Lol.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Galaxy.Lol.Infraestructure.Configuration.Repositories.Entities
{
    public class ChampionAbilityConfiguration : IEntityTypeConfiguration<ChampionAbility>
    {
        public void Configure(EntityTypeBuilder<ChampionAbility> builder)
        {
            builder.ToTable("champion_ability");

            builder.HasKey(a => a.Id);
            builder.Property(a => a.Id).HasColumnName("id");
            builder.Property(a => a.ChampionProfileId).HasColumnName("champion_profile_id");
            builder.Property(a => a.Slot).HasColumnName("slot").HasConversion<string>().HasMaxLength(20);
            builder.Property(a => a.Name).HasColumnName("name").HasMaxLength(120).IsRequired();
            builder.Property(a => a.Description).HasColumnName("description").HasMaxLength(2000);
            builder.Property(a => a.ImageUrl).HasColumnName("image_url").HasMaxLength(400);
            builder.Property(a => a.Cooldown).HasColumnName("cooldown");
            builder.Property(a => a.IsActive).HasColumnName("is_active");
            builder.Property(a => a.CreatedAt).HasColumnName("created_at");
            builder.Property(a => a.UpdatedAt).HasColumnName("updated_at");

            builder.HasIndex(a => new { a.ChampionProfileId, a.Slot })
                   .HasDatabaseName("ix_champion_ability_profile_slot");
        }
    }
}
