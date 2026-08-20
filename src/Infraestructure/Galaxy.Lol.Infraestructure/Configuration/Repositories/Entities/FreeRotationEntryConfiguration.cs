using Galaxy.Lol.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Galaxy.Lol.Infraestructure.Configuration.Repositories.Entities
{
    public class FreeRotationEntryConfiguration : IEntityTypeConfiguration<FreeRotationEntry>
    {
        public void Configure(EntityTypeBuilder<FreeRotationEntry> builder)
        {
            builder.ToTable("free_rotation_entry");

            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).HasColumnName("id");
            builder.Property(e => e.FreeRotationId).HasColumnName("free_rotation_id");
            builder.Property(e => e.ForNewPlayers).HasColumnName("for_new_players");
            builder.Property(e => e.IsActive).HasColumnName("is_active");
            builder.Property(e => e.CreatedAt).HasColumnName("created_at");
            builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            builder.OwnsOne(e => e.Key, key =>
            {
                key.Property(k => k.Value).HasColumnName("champion_key").IsRequired();
                key.HasIndex(k => k.Value).HasDatabaseName("ix_free_rotation_entry_key");
            });
        }
    }
}
