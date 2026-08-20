using Galaxy.Lol.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Galaxy.Lol.Infraestructure.Configuration.Repositories.Entities
{

    public class ChampionProfileConfiguration : IEntityTypeConfiguration<ChampionProfile>
    {
        public void Configure(EntityTypeBuilder<ChampionProfile> builder)
        {
            builder.ToTable("champion_profile");

            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id).HasColumnName("id");
            builder.Property(p => p.ChampionId).HasColumnName("champion_id").HasMaxLength(60).IsRequired();
            builder.Property(p => p.Name).HasColumnName("name").HasMaxLength(80).IsRequired();
            builder.Property(p => p.Title).HasColumnName("title").HasMaxLength(160);
            builder.Property(p => p.Blurb).HasColumnName("blurb").HasMaxLength(1200);
            builder.Property(p => p.ImageUrl).HasColumnName("image_url").HasMaxLength(400);
            builder.Property(p => p.Version).HasColumnName("version").HasMaxLength(30);
            builder.Property(p => p.IsActive).HasColumnName("is_active");
            builder.Property(p => p.CreatedAt).HasColumnName("created_at");
            builder.Property(p => p.UpdatedAt).HasColumnName("updated_at");

            builder.OwnsOne(p => p.Key, key =>
            {
                key.Property(k => k.Value).HasColumnName("champion_key").IsRequired();
                key.HasIndex(k => k.Value).IsUnique().HasDatabaseName("ux_champion_profile_key");
            });

            builder.OwnsOne(p => p.Difficulty, dif =>
            {
                dif.Property(d => d.Value).HasColumnName("difficulty").IsRequired();
            });

            builder.OwnsOne(p => p.Stats, stats =>
            {
                stats.Property(s => s.Hp).HasColumnName("stat_hp");
                stats.Property(s => s.Mp).HasColumnName("stat_mp");
                stats.Property(s => s.Armor).HasColumnName("stat_armor");
                stats.Property(s => s.SpellBlock).HasColumnName("stat_spell_block");
                stats.Property(s => s.AttackDamage).HasColumnName("stat_attack_damage");
                stats.Property(s => s.AttackSpeed).HasColumnName("stat_attack_speed");
                stats.Property(s => s.MoveSpeed).HasColumnName("stat_move_speed");
            });

            builder.OwnsMany(p => p.Roles, role =>
            {
                role.ToTable("champion_role");
                role.WithOwner().HasForeignKey("champion_profile_id");
                role.Property<int>("id").ValueGeneratedOnAdd();
                role.HasKey("id");
                role.Property(r => r.Value).HasColumnName("role").HasMaxLength(30).IsRequired();
                role.HasIndex(r => r.Value).HasDatabaseName("ix_champion_role_role");
            });

            builder.HasMany(p => p.Abilities)
                   .WithOne()
                   .HasForeignKey(a => a.ChampionProfileId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(p => p.ChampionId).IsUnique().HasDatabaseName("ux_champion_profile_champion_id");

            builder.Metadata.FindNavigation(nameof(ChampionProfile.Abilities))!
                   .SetPropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
