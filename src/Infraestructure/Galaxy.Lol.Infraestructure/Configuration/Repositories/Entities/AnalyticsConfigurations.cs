using Galaxy.Lol.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Galaxy.Lol.Infraestructure.Configuration.Repositories.Entities
{

    public class SyncLogConfiguration : IEntityTypeConfiguration<SyncLog>
    {
        public void Configure(EntityTypeBuilder<SyncLog> builder)
        {
            builder.ToTable("SincronizacionLog", "analitica");

            builder.HasKey(l => l.Id);
            builder.Property(l => l.Origin).HasConversion<string>().HasMaxLength(20);
            builder.Property(l => l.Endpoint).HasMaxLength(120).IsRequired();
            builder.Property(l => l.Platform).HasMaxLength(10);
            builder.Property(l => l.Message).HasMaxLength(1000);
            builder.Property(l => l.ExecutedAt);

            builder.HasIndex(l => l.ExecutedAt).HasDatabaseName("IX_SincronizacionLog_ExecutedAt");
        }
    }

    public class MasterySnapshotConfiguration : IEntityTypeConfiguration<MasterySnapshot>
    {
        public void Configure(EntityTypeBuilder<MasterySnapshot> builder)
        {
            builder.ToTable("MaestriaSnapshot", "analitica");

            builder.HasKey(s => s.Id);
            builder.Property(s => s.MaskedPuuid).HasMaxLength(20).IsRequired();
            builder.Property(s => s.ChampionName).HasMaxLength(80);
            builder.Property(s => s.DominanceIndex).HasColumnType("decimal(5,2)");

            builder.HasIndex(s => new { s.MaskedPuuid, s.ChampionKey, s.TakenAt })
                   .HasDatabaseName("IX_MaestriaSnapshot_Jugador");
        }
    }
}
