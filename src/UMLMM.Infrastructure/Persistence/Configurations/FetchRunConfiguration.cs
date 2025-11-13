using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UMLMM.Domain.Entities;

namespace UMLMM.Infrastructure.Persistence.Configurations;

public class FetchRunConfiguration : IEntityTypeConfiguration<FetchRun>
{
    public void Configure(EntityTypeBuilder<FetchRun> builder)
    {
        builder.ToTable("fetch_runs");

        builder.HasKey(fr => fr.RunId);
        builder.Property(fr => fr.RunId).HasColumnName("run_id");

        builder.Property(fr => fr.SourceId)
            .HasColumnName("source_id")
            .IsRequired();

        builder.Property(fr => fr.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(fr => fr.StartedAt)
            .HasColumnName("started_at")
            .IsRequired();

        builder.Property(fr => fr.EndedAt)
            .HasColumnName("ended_at");

        builder.Property(fr => fr.Stats)
            .HasColumnName("stats")
            .HasColumnType("jsonb");

        builder.Property(fr => fr.ErrorText)
            .HasColumnName("error_text");

        builder.Property(fr => fr.CreatedAtUtc)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasOne(fr => fr.Source)
            .WithMany(s => s.FetchRuns)
            .HasForeignKey(fr => fr.SourceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(fr => fr.SourceId);
        builder.HasIndex(fr => fr.Status);
        builder.HasIndex(fr => fr.StartedAt);
    }
}
