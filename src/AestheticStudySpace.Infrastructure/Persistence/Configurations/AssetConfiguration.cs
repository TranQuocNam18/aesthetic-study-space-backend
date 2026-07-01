using AestheticStudySpace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AestheticStudySpace.Infrastructure.Persistence.Configurations;

public class AssetConfiguration : IEntityTypeConfiguration<Asset>
{
    public void Configure(EntityTypeBuilder<Asset> builder)
    {
        builder.ToTable("Assets");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Name).HasMaxLength(120).IsRequired();
        builder.Property(a => a.Description).HasMaxLength(500);
        builder.Property(a => a.Url).HasMaxLength(2048).IsRequired();
        builder.Property(a => a.PreviewUrl).HasMaxLength(2048);
        builder.Property(a => a.AssetType).HasConversion<string>().HasMaxLength(20);
        builder.Property(a => a.Category).HasMaxLength(30).IsRequired();
        builder.HasIndex(a => a.AssetType);
        builder.HasIndex(a => a.Category);
    }
}
