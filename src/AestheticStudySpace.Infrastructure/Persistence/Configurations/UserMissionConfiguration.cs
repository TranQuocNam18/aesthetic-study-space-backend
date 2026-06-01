using AestheticStudySpace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AestheticStudySpace.Infrastructure.Persistence.Configurations;

public class UserMissionConfiguration : IEntityTypeConfiguration<UserMission>
{
    public void Configure(EntityTypeBuilder<UserMission> builder)
    {
        builder.ToTable("UserMissions");
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Mission)
            .WithMany()
            .HasForeignKey(x => x.MissionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.UserId, x.MissionId, x.PeriodDate }).IsUnique();
        builder.HasIndex(x => x.IsCompleted);
        builder.HasIndex(x => x.ClaimedAt);
    }
}

