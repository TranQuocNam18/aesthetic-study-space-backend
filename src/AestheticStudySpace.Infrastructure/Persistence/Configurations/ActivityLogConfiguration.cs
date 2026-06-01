using AestheticStudySpace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AestheticStudySpace.Infrastructure.Persistence.Configurations;

public class ActivityLogConfiguration : IEntityTypeConfiguration<ActivityLog>
{
    public void Configure(EntityTypeBuilder<ActivityLog> builder)
    {
        builder.ToTable("ActivityLogs");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Action).HasMaxLength(120).IsRequired();
        builder.Property(x => x.EntityName).HasMaxLength(80);
        builder.Property(x => x.EntityId).HasMaxLength(80);
        builder.Property(x => x.MetadataJson).HasColumnType("text");
        builder.Property(x => x.IpAddress).HasMaxLength(60);
        builder.Property(x => x.UserAgent).HasMaxLength(500);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => new { x.Action, x.CreatedAt });
    }
}

