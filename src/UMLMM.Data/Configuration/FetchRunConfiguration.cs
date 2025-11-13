using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UMLMM.Core.Domain.Entities;

namespace UMLMM.Data.Configuration;

public class FetchRunConfiguration : IEntityTypeConfiguration<FetchRun>
{
    public void Configure(EntityTypeBuilder<FetchRun> builder)
    {
        builder.ToTable("fetch_runs");
        
        builder.HasKey(fr => fr.Id);
        builder.Property(fr => fr.Id).HasColumnName("id");
        
        builder.Property(fr => fr.SourceId)
            .HasColumnName("source_id")
            .IsRequired();
        
        builder.Property(fr => fr.StartedAt)
            .HasColumnName("started_at")
            .IsRequired();
        
        builder.Property(fr => fr.CompletedAt)
            .HasColumnName("completed_at");
        
        builder.Property(fr => fr.PostsFetched)
            .HasColumnName("posts_fetched")
            .IsRequired();
        
        builder.Property(fr => fr.PostsCreated)
            .HasColumnName("posts_created")
            .IsRequired();
        
        builder.Property(fr => fr.PostsUpdated)
            .HasColumnName("posts_updated")
            .IsRequired();
        
        builder.Property(fr => fr.Success)
            .HasColumnName("success")
            .IsRequired();
        
        builder.Property(fr => fr.ErrorMessage)
            .HasColumnName("error_message");
        
        builder.HasOne(fr => fr.Source)
            .WithMany(s => s.FetchRuns)
            .HasForeignKey(fr => fr.SourceId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasIndex(fr => fr.StartedAt);
    }
}
