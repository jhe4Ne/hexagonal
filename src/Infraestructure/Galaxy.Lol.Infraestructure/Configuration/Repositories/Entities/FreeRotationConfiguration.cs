using Galaxy.Lol.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Galaxy.Lol.Infraestructure.Configuration.Repositories.Entities
{
    public class FreeRotationConfiguration : IEntityTypeConfiguration<FreeRotation>
    {
        public void Configure(EntityTypeBuilder<FreeRotation> builder)
        {
            builder.ToTable("free_rotation");

            builder.HasKey(r => r.Id);
            builder.Property(r => r.Id).HasColumnName("id");
            builder.Property(r => r.Platform).HasColumnName("platform").HasMaxLength(10).IsRequired();
            builder.Property(r => r.MaxNewPlayerLevel).HasColumnName("max_new_player_level");
            builder.Property(r => r.Hash).HasColumnName("hash").HasMaxLength(64).IsRequired();
            builder.Property(r => r.IsActive).HasColumnName("is_active");
            builder.Property(r => r.CreatedAt).HasColumnName("created_at");
            builder.Property(r => r.UpdatedAt).HasColumnName("updated_at");

            builder.OwnsOne(r => r.Period, period =>
            {
                period.Property(p => p.Start).HasColumnName("period_start");
                period.Property(p => p.End).HasColumnName("period_end");
            });

            builder.HasMany(r => r.Entries)
                   .WithOne()
                   .HasForeignKey(e => e.FreeRotationId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(r => new { r.Platform, r.Hash })
                   .IsUnique()
                   .HasDatabaseName("ux_free_rotation_platform_hash");

            builder.Metadata.FindNavigation(nameof(FreeRotation.Entries))!
                   .SetPropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
