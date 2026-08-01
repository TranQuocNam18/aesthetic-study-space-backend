using AestheticStudySpace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AestheticStudySpace.Infrastructure.Persistence.Configurations;

public class UserLuckyDrawConfiguration : IEntityTypeConfiguration<UserLuckyDraw>
{
    public void Configure(EntityTypeBuilder<UserLuckyDraw> builder)
    {
        builder.ToTable("UserLuckyDraws");
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.RewardDescription).HasMaxLength(255);

        builder.HasIndex(x => new { x.UserId, x.DrawDate });
    }
}
