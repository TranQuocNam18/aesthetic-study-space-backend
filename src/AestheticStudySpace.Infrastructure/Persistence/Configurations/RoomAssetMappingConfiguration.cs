using AestheticStudySpace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AestheticStudySpace.Infrastructure.Persistence.Configurations;

public class RoomAssetMappingConfiguration : IEntityTypeConfiguration<RoomAssetMapping>
{
    public void Configure(EntityTypeBuilder<RoomAssetMapping> builder)
    {
        builder.ToTable("RoomAssetMappings");
        builder.HasKey(m => m.Id);
        builder.HasIndex(m => new { m.RoomId, m.AssetId }).IsUnique();
        builder.HasOne(m => m.Room).WithMany(r => r.AssetMappings).HasForeignKey(m => m.RoomId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.Asset).WithMany(a => a.RoomMappings).HasForeignKey(m => m.AssetId).OnDelete(DeleteBehavior.Restrict);
    }
}
