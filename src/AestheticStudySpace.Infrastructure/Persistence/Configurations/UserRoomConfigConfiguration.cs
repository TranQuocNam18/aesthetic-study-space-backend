using AestheticStudySpace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AestheticStudySpace.Infrastructure.Persistence.Configurations;

public class UserRoomConfigConfiguration : IEntityTypeConfiguration<UserRoomConfig>
{
    public void Configure(EntityTypeBuilder<UserRoomConfig> builder)
    {
        builder.ToTable("UserRoomConfigs");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.JsonConfig).IsRequired();
        builder.HasIndex(c => new { c.UserId, c.RoomId }).IsUnique();
        builder.HasOne(c => c.User).WithMany(u => u.RoomConfigs).HasForeignKey(c => c.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(c => c.Room).WithMany(r => r.UserConfigs).HasForeignKey(c => c.RoomId).OnDelete(DeleteBehavior.Cascade);
    }
}
